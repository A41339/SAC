using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using FGA.Model;

namespace FGA.Services
{
    public class BalanzaComprobacionService
    {
        private readonly CultureInfo _crCulture = new CultureInfo("es-CR");

        /// <summary>
        /// Construye el modelo completo de la Balanza de Comprobación a 5 períodos en modalidad Acumulado o Mensual.
        /// </summary>
        public BalanzaComprobacionViewModel ConstruirModeloBalanza(
            string entidadId,
            string periodoSel,
            string modalidadSel,
            string tipoCompSel,
            HttpSessionStateBase session,
            FGA_En_Linea.SPService.SPClient spClient = null,
            FGA_En_Linea.EntidadService.EntidadServiceClient entClient = null,
            FGA_En_Linea.CatalogoCuentaService.CatalogoCuentaClient catClient = null)
        {
            bool disposeSp = false;
            if (spClient == null)
            {
                spClient = new FGA_En_Linea.SPService.SPClient();
                disposeSp = true;
            }

            bool disposeEnt = false;
            if (entClient == null)
            {
                entClient = new FGA_En_Linea.EntidadService.EntidadServiceClient();
                disposeEnt = true;
            }

            bool disposeCat = false;
            if (catClient == null)
            {
                catClient = new FGA_En_Linea.CatalogoCuentaService.CatalogoCuentaClient();
                disposeCat = true;
            }

            try
            {
                var idEntidad = string.IsNullOrEmpty(entidadId) ? (session["IdEntidad"] != null ? session["IdEntidad"].ToString() : "2") : entidadId;
                session["IdEntidad"] = idEntidad;

                var modalidad = string.IsNullOrEmpty(modalidadSel) ? (session["BalanzaModalidad"] != null ? session["BalanzaModalidad"].ToString() : "Acumulado") : modalidadSel;
                if (!modalidad.Equals("Mensual", StringComparison.OrdinalIgnoreCase))
                {
                    modalidad = "Acumulado";
                }
                session["BalanzaModalidad"] = modalidad;

                var tipoComp = string.IsNullOrEmpty(tipoCompSel) ? (session["TipoComparacion"] != null ? session["TipoComparacion"].ToString() : "Interanual") : tipoCompSel;
                session["TipoComparacion"] = tipoComp;

                var periodoRef = string.IsNullOrEmpty(periodoSel) ? (session["Periodo2"] != null ? session["Periodo2"].ToString() : (session["Periodo"] != null ? session["Periodo"].ToString() : DateTime.Now.ToString("MM/yyyy"))) : periodoSel;

                DateTime refDate;
                try
                {
                    refDate = Utility.Utilitarios.ConvertirAFecha(periodoRef);
                }
                catch
                {
                    refDate = DateTime.Now;
                }
                session["Periodo2"] = refDate.ToShortDateString();

                // Verificación de caché en memoria
                string cacheKey = string.Format("FGA_BalanzaComp_{0}_{1:yyyyMM}_{2}_{3}", idEntidad, refDate, modalidad, tipoComp);
                if (HttpRuntime.Cache != null)
                {
                    var cachedModel = HttpRuntime.Cache.Get(cacheKey) as BalanzaComprobacionViewModel;
                    if (cachedModel != null)
                    {
                        return cachedModel;
                    }
                }

                var model = new BalanzaComprobacionViewModel
                {
                    EntidadId = idEntidad,
                    Modalidad = modalidad,
                    TipoComparacion = tipoComp,
                    PeriodoReferencia = refDate,
                    LogoUrl = session["Logo"] != null ? session["Logo"].ToString() : ""
                };

                // Nombre de la Entidad
                if (idEntidad == "-1")
                {
                    model.NombreEntidad = "TODAS LAS COOPERATIVAS";
                }
                else
                {
                    if (session != null && session["NomEntidad"] != null && session["IdEntidad"] != null && session["IdEntidad"].ToString() == idEntidad && !string.IsNullOrEmpty(session["NomEntidad"].ToString()))
                    {
                        model.NombreEntidad = session["NomEntidad"].ToString();
                    }
                    else
                    {
                        var entObj = entClient != null ? entClient.Get(idEntidad) : null;
                        model.NombreEntidad = entObj != null ? entObj.Nombre : (session != null && session["NomEntidad"] != null ? session["NomEntidad"].ToString() : "COOPENAE R.L.");
                        if (session != null) session["NomEntidad"] = model.NombreEntidad;
                    }
                }

                // 1. Determinar 5 períodos consecutivos según tipo de comparación
                DateTime[] pFechas = DeterminarFechasPeriodos(refDate, tipoComp, model);
                model.FechasPeriodos = pFechas;

                // 2. Encabezados descriptivos
                for (int i = 0; i < 5; i++)
                {
                    string mesNom = Utility.Utilitarios.toUpperFirstLetter(pFechas[i].ToString("MMM. yyyy", _crCulture));
                    if (tipoComp.Equals("Trimestral", StringComparison.OrdinalIgnoreCase))
                    {
                        if (pFechas[i].Month == 3) mesNom += " (I Trim)";
                        else if (pFechas[i].Month == 6) mesNom += " (II Trim)";
                        else if (pFechas[i].Month == 9) mesNom += " (III Trim)";
                        else if (pFechas[i].Month == 12) mesNom += " (IV Trim)";
                    }
                    model.EncabezadosPeriodos[i] = mesNom;
                }

                // 3. Obtener catálogo contable completo (con caché para máxima velocidad)
                var catalogo = ObtenerCatalogoCuentas(catClient);
                var catDict = new Dictionary<string, FGA.Models.CatalogoCuenta>(StringComparer.OrdinalIgnoreCase);
                if (catalogo != null)
                {
                    foreach (var c in catalogo)
                    {
                        if (!string.IsNullOrEmpty(c.Cuenta) && !catDict.ContainsKey(c.Cuenta.Trim()))
                        {
                            catDict.Add(c.Cuenta.Trim(), c);
                        }
                    }
                }

                // 4. Consultar saldos a 5 períodos mediante el nuevo SP optimizado o fallback a SPClient
                List<FilaBalanzaItem> filasTemp = null;
                try
                {
                    var spData = spClient.FGA_Rpt_Balanza_Comprobacion_5Periodos(idEntidad, refDate, modalidad, tipoComp);
                    if (spData != null && spData.Length > 0)
                    {
                        filasTemp = new List<FilaBalanzaItem>();
                        int ord = 1;
                        foreach (var row in spData)
                        {
                            filasTemp.Add(new FilaBalanzaItem
                            {
                                Orden = row.Orden ?? ord++,
                                Cuenta = row.Cuenta != null ? row.Cuenta.Trim() : "",
                                Nombre = row.Nombre != null ? row.Nombre.Trim() : "",
                                Nivel = row.Nivel ?? DeterminarNivel(row.Cuenta),
                                Padre = row.Padre != null ? row.Padre.Trim() : DeterminarPadre(row.Cuenta),
                                Periodos = new decimal[] {
                                    row.P1 ?? 0m,
                                    row.P2 ?? 0m,
                                    row.P3 ?? 0m,
                                    row.P4 ?? 0m,
                                    row.P5 ?? 0m
                                },
                                VariacionAbsoluta = row.VariacionAbsoluta ?? 0m,
                                VariacionRelativa = row.VariacionRelativa ?? 0m
                            });
                        }
                    }
                }
                catch (Exception)
                {
                    filasTemp = null;
                }

                // Si el SP aún no está compilado en la base de datos o falló, ejecutar el respaldo mediante rango
                if (filasTemp == null || filasTemp.Count == 0)
                {
                    bool isMensual = modalidad.Equals("Mensual", StringComparison.OrdinalIgnoreCase);
                    var saldosPorCuenta = new Dictionary<string, decimal[]>(StringComparer.OrdinalIgnoreCase);
                    var nombresPorCuenta = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                    for (int c = 0; c < 5; c++)
                    {
                        DateTime fechaCorte = pFechas[c];
                        try
                        {
                            var tak = spClient.FGA_Consultar_Balance_Comprobacion_Rango(idEntidad, fechaCorte, fechaCorte, isMensual, false);
                            if (tak != null && tak.Length > 0)
                            {
                                foreach (var row in tak)
                                {
                                    if (row == null || string.IsNullOrEmpty(row.VALORES) || row.VALORES.Contains("Periodo"))
                                        continue;

                                    string cuenta = !string.IsNullOrEmpty(row.CUENTA) ? row.CUENTA.Trim() : null;
                                    string nombre = !string.IsNullOrEmpty(row.NOMBRE) ? row.NOMBRE.Trim() : null;
                                    decimal monto = 0m;

                                    var parts = row.VALORES.Split(';');
                                    if (parts.Length >= 2)
                                    {
                                        if (string.IsNullOrEmpty(cuenta) && parts.Length >= 2)
                                        {
                                            cuenta = parts[1].Trim();
                                            if (string.IsNullOrEmpty(nombre)) nombre = parts[0].Trim();
                                        }

                                        for (int p = 2; p < parts.Length; p++)
                                        {
                                            string token = parts[p].Trim();
                                            if (string.IsNullOrEmpty(token)) continue;

                                            if (decimal.TryParse(token, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal valInv))
                                            {
                                                monto = valInv;
                                                break;
                                            }
                                            else if (decimal.TryParse(token, NumberStyles.Any, _crCulture, out decimal valCr))
                                            {
                                                monto = valCr;
                                                break;
                                            }
                                        }
                                    }

                                    if (string.IsNullOrEmpty(cuenta))
                                        continue;

                                    if (!saldosPorCuenta.ContainsKey(cuenta))
                                    {
                                        saldosPorCuenta[cuenta] = new decimal[5];
                                    }

                                    saldosPorCuenta[cuenta][c] = Math.Round(monto / 1000000m, 2);

                                    if (!string.IsNullOrEmpty(nombre) && !nombresPorCuenta.ContainsKey(cuenta))
                                    {
                                        nombresPorCuenta[cuenta] = nombre;
                                    }
                                }
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }

                    var todasCuentas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    foreach (var k in saldosPorCuenta.Keys) todasCuentas.Add(k);
                    foreach (var k in catDict.Keys) todasCuentas.Add(k);

                    filasTemp = new List<FilaBalanzaItem>();

                    foreach (var cuenta in todasCuentas)
                    {
                        decimal[] periodos = saldosPorCuenta.ContainsKey(cuenta) ? saldosPorCuenta[cuenta] : new decimal[5];
                        bool tieneSaldo = periodos.Any(x => x != 0m);

                        FGA.Models.CatalogoCuenta catItem = null;
                        catDict.TryGetValue(cuenta, out catItem);

                        int nivel = catItem?.Nivel ?? DeterminarNivel(cuenta);
                        string padre = catItem?.Padre ?? DeterminarPadre(cuenta);
                        string nombre = catItem != null && !string.IsNullOrEmpty(catItem.Nombre)
                            ? catItem.Nombre
                            : (nombresPorCuenta.ContainsKey(cuenta) ? nombresPorCuenta[cuenta] : "Cuenta " + cuenta);

                        if (!tieneSaldo && nivel > 2)
                            continue;

                        var itemFila = new FilaBalanzaItem
                        {
                            Cuenta = cuenta,
                            Nombre = nombre,
                            Nivel = nivel,
                            Padre = padre,
                            Periodos = periodos
                        };

                        filasTemp.Add(itemFila);
                    }
                }

                // 6. Ordenar jerárquicamente por código contable
                filasTemp = filasTemp.OrderBy(x => x.Cuenta, StringComparer.OrdinalIgnoreCase).ToList();

                // Marcar cuentas que son Padres de otras en la lista
                var codigosPresentes = new HashSet<string>(filasTemp.Select(x => x.Cuenta), StringComparer.OrdinalIgnoreCase);
                var padresPresentes = new HashSet<string>(filasTemp.Where(x => !string.IsNullOrEmpty(x.Padre)).Select(x => x.Padre), StringComparer.OrdinalIgnoreCase);

                int orden = 1;
                foreach (var f in filasTemp)
                {
                    f.Orden = orden++;
                    f.EsPadre = padresPresentes.Contains(f.Cuenta);
                }

                // 7. Calcular Totales Principales para Análisis Vertical y Cuadre
                decimal[] totActivos = ObtenerSaldosClase(filasTemp, "1");
                decimal[] totPasivos = ObtenerSaldosClase(filasTemp, "2");
                decimal[] totPatrimonio = ObtenerSaldosClase(filasTemp, "3");
                decimal[] totIngresos = ObtenerSaldosClase(filasTemp, "4");
                decimal[] totGastos = ObtenerSaldosClase(filasTemp, "5");

                // Variaciones Absolutas y Relativas
                int varRefCol = tipoComp.Equals("Interanual", StringComparison.OrdinalIgnoreCase) ? 1 : 3;
                foreach (var f in filasTemp)
                {
                    decimal vAct = f.Periodos[4];
                    decimal vRef = f.Periodos[varRefCol];
                    f.VariacionAbsoluta = Math.Round(vAct - vRef, 2);
                    f.VariacionRelativa = vRef != 0 ? Math.Round(((vAct - vRef) / Math.Abs(vRef)) * 100m, 2) : 0m;

                    // Análisis Vertical (% Representa)
                    for (int c = 0; c < 5; c++)
                    {
                        decimal baseTotal = 0m;
                        if (f.Cuenta.StartsWith("1"))
                        {
                            baseTotal = totActivos[c];
                        }
                        else if (f.Cuenta.StartsWith("2") || f.Cuenta.StartsWith("3"))
                        {
                            baseTotal = totPasivos[c] + totPatrimonio[c];
                            if (baseTotal == 0m) baseTotal = f.Cuenta.StartsWith("2") ? totPasivos[c] : totPatrimonio[c];
                        }
                        else
                        {
                            baseTotal = totActivos[c];
                        }

                        f.PorcentajesRepresenta[c] = baseTotal != 0 ? Math.Round((f.Periodos[c] / baseTotal) * 100m, 2) : 0m;
                    }

                    model.Filas.Add(f);
                }

                // 8. Cuadre Contable (Activo = Pasivo + Patrimonio)
                decimal actUlt = totActivos[4];
                decimal pasUlt = totPasivos[4];
                decimal patUlt = totPatrimonio[4];
                decimal diffCuadre = actUlt - (pasUlt + patUlt);

                model.EstaCuadrada = Math.Abs(diffCuadre) <= 0.05m;
                model.MensajeCuadre = model.EstaCuadrada
                    ? "Balanza Contable Cuadrada (Activo = Pasivo + Patrimonio)"
                    : string.Format("Diferencia de cuadre contable: ₡ {0:N2} millones", diffCuadre);

                // 9. DTO de Composición para Gráficos Ejecutivos
                model.DatosComposicion = ConstruirComposicionDTO(totActivos, totPasivos, totPatrimonio, totIngresos, totGastos, pFechas);

                // Guardar en caché 10 minutos
                if (HttpRuntime.Cache != null && model.Filas != null && model.Filas.Count > 0)
                {
                    HttpRuntime.Cache.Insert(
                        cacheKey,
                        model,
                        null,
                        DateTime.UtcNow.AddMinutes(10),
                        System.Web.Caching.Cache.NoSlidingExpiration);
                }

                return model;
            }
            finally
            {
                if (disposeSp) { try { spClient.Close(); } catch { } }
                if (disposeEnt) { try { entClient.Close(); } catch { } }
                if (disposeCat) { try { catClient.Close(); } catch { } }
            }
        }

        private DateTime[] DeterminarFechasPeriodos(DateTime refDate, string tipoComp, BalanzaComprobacionViewModel model)
        {
            DateTime[] pFechas = new DateTime[5];
            int y = refDate.Year;
            int m = refDate.Month;

            if (tipoComp.Equals("Mensual", StringComparison.OrdinalIgnoreCase))
            {
                pFechas[4] = new DateTime(y, m, 1);
                pFechas[3] = pFechas[4].AddMonths(-1);
                pFechas[2] = pFechas[4].AddMonths(-2);
                pFechas[1] = pFechas[4].AddMonths(-3);
                pFechas[0] = pFechas[4].AddMonths(-4);
                model.ComparacionTitulo = string.Format("Balanza {0} - Comparativo Mensual", model.Modalidad);
                model.ComparacionSubtitulo = "Cifras en millones de colones (5 meses consecutivos)";
            }
            else if (tipoComp.Equals("Trimestral", StringComparison.OrdinalIgnoreCase))
            {
                int qMonth = ((m - 1) / 3 + 1) * 3;
                DateTime qCurrent = new DateTime(y, qMonth, 1);
                pFechas[4] = qCurrent;
                pFechas[3] = qCurrent.AddMonths(-3);
                pFechas[2] = qCurrent.AddMonths(-6);
                pFechas[1] = qCurrent.AddMonths(-9);
                pFechas[0] = qCurrent.AddMonths(-12);
                model.ComparacionTitulo = string.Format("Balanza {0} - Comparativo Trimestral", model.Modalidad);
                model.ComparacionSubtitulo = "Cifras en millones de colones (5 trimestres consecutivos)";
            }
            else // Interanual
            {
                pFechas[4] = new DateTime(y, m, 1);
                pFechas[3] = pFechas[4].AddYears(-1);
                pFechas[2] = pFechas[4].AddYears(-2);
                pFechas[1] = pFechas[4].AddYears(-3);
                pFechas[0] = pFechas[4].AddYears(-4);
                model.ComparacionTitulo = string.Format("Balanza {0} - Comparativo Interanual", model.Modalidad);
                model.ComparacionSubtitulo = "Cifras en millones de colones (5 años consecutivos)";
            }

            return pFechas;
        }

        private FGA.Models.CatalogoCuenta[] ObtenerCatalogoCuentas(FGA_En_Linea.CatalogoCuentaService.CatalogoCuentaClient catClient)
        {
            const string cacheKey = "FGA_CatalogoCuentas_Balanza";
            if (HttpRuntime.Cache != null)
            {
                var cached = HttpRuntime.Cache.Get(cacheKey) as FGA.Models.CatalogoCuenta[];
                if (cached != null) return cached;
            }

            try
            {
                var lista = catClient.GetAll();
                if (lista != null && HttpRuntime.Cache != null)
                {
                    HttpRuntime.Cache.Insert(cacheKey, lista, null, DateTime.UtcNow.AddHours(1), System.Web.Caching.Cache.NoSlidingExpiration);
                }
                return lista;
            }
            catch
            {
                return new FGA.Models.CatalogoCuenta[0];
            }
        }

        private int DeterminarNivel(string cuenta)
        {
            if (string.IsNullOrEmpty(cuenta)) return 4;
            cuenta = cuenta.Trim();
            if (cuenta.Length == 1 || (cuenta.Length == 3 && cuenta.EndsWith("00"))) return 1; // Clase
            if (cuenta.Length == 3 && cuenta.EndsWith("0")) return 2; // Grupo
            if (cuenta.Length <= 4 && !cuenta.Contains(".")) return 3; // Subgrupo
            return 4; // Cuenta Detalle / Analítica
        }

        private string DeterminarPadre(string cuenta)
        {
            if (string.IsNullOrEmpty(cuenta)) return "";
            cuenta = cuenta.Trim();
            if (cuenta.Contains("."))
            {
                int dotIdx = cuenta.LastIndexOf('.');
                return cuenta.Substring(0, dotIdx);
            }
            if (cuenta.Length >= 4)
            {
                return cuenta.Substring(0, 3);
            }
            if (cuenta.Length == 3 && !cuenta.EndsWith("00"))
            {
                return cuenta.Substring(0, 1) + "00";
            }
            return "";
        }

        private decimal[] ObtenerSaldosClase(List<FilaBalanzaItem> filas, string digitoClase)
        {
            decimal[] saldos = new decimal[5];
            string ctaPrincipal = digitoClase + "00";

            var filaClase = filas.FirstOrDefault(x => x.Cuenta == ctaPrincipal || (x.Cuenta == digitoClase && x.Nivel == 1));
            if (filaClase != null && filaClase.Periodos.Any(p => p != 0))
            {
                return filaClase.Periodos;
            }

            // Si no está la cuenta de clase mayor, sumar los grupos principales (Nivel 2)
            var grupos = filas.Where(x => x.Cuenta.StartsWith(digitoClase) && x.Nivel == 2).ToList();
            if (grupos.Count > 0)
            {
                for (int c = 0; c < 5; c++)
                {
                    saldos[c] = grupos.Sum(g => g.Periodos[c]);
                }
                return saldos;
            }

            // O sumar todas las cuentas de Nivel 1 o Nivel 3 si no hay grupos
            var subgrupos = filas.Where(x => x.Cuenta.StartsWith(digitoClase) && x.Nivel == 3).ToList();
            if (subgrupos.Count > 0)
            {
                for (int c = 0; c < 5; c++)
                {
                    saldos[c] = subgrupos.Sum(s => s.Periodos[c]);
                }
            }

            return saldos;
        }

        private ComposicionBalanzaDTO ConstruirComposicionDTO(
            decimal[] totActivos,
            decimal[] totPasivos,
            decimal[] totPatrimonio,
            decimal[] totIngresos,
            decimal[] totGastos,
            DateTime[] pFechas)
        {
            var comp = new ComposicionBalanzaDTO();

            for (int i = 0; i < 5; i++)
            {
                comp.PeriodosCategorias[i] = Utility.Utilitarios.toUpperFirstLetter(pFechas[i].ToString("MMM. yyyy", _crCulture));
                comp.SerieActivos[i] = totActivos[i];
                comp.SeriePasivos[i] = totPasivos[i];
                comp.SeriePatrimonio[i] = totPatrimonio[i];
                comp.SerieIngresos[i] = totIngresos[i];
                comp.SerieGastos[i] = totGastos[i];
            }

            comp.TotalActivoUltimo = totActivos[4];
            comp.TotalPasivoUltimo = totPasivos[4];
            comp.TotalPatrimonioUltimo = totPatrimonio[4];
            comp.TotalIngresosUltimo = totIngresos[4];
            comp.TotalGastosUltimo = totGastos[4];

            decimal baseActivo = comp.TotalActivoUltimo;
            if (baseActivo > 0)
            {
                comp.PorcActivo = 100m;
                comp.PorcPasivo = Math.Round((comp.TotalPasivoUltimo / baseActivo) * 100m, 2);
                comp.PorcPatrimonio = Math.Round((comp.TotalPatrimonioUltimo / baseActivo) * 100m, 2);
            }

            comp.VarActivo = Math.Round(totActivos[4] - totActivos[3], 2);
            comp.VarPasivo = Math.Round(totPasivos[4] - totPasivos[3], 2);
            comp.VarPatrimonio = Math.Round(totPatrimonio[4] - totPatrimonio[3], 2);
            comp.VarIngresos = Math.Round(totIngresos[4] - totIngresos[3], 2);
            comp.VarGastos = Math.Round(totGastos[4] - totGastos[3], 2);

            return comp;
        }
    }
}
