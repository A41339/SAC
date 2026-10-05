using FGA.Models;
using FGA.Utility;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace FGA.Controllers
{
    public class BaseController : Controller
    {
        protected void Load()
        {
            try
            {             
                Entidad[] rawLista = Session["AllEntidades"] as Entidad[];
                if (rawLista == null)
                {
                    rawLista = ent.GetAll();
                    Session["AllEntidades"] = rawLista;
                }

                Usuario ObjUser = Session["CurrentUserObj"] as Usuario;
                if (ObjUser == null)
                {
                    ObjUser = usr.Get(Env.GetUserInfo("userid"));
                    Session["CurrentUserObj"] = ObjUser;
                }

                Entidad[] lista = (Entidad[])rawLista.Clone();
                String ruta = string.Empty;
                if (ObjUser.Entidad_Usuario_Id == Utility.Utilitarios.entidadAdministradora)
                {                   
                    lista = lista.Where(o => o.Id != Utility.Utilitarios.entidadAdministradora && o.Activo == true).OrderBy(o => o.Nombre).ToArray();
                    
                    if (Session["IdEntidad"] is null || Session["IdEntidad"].ToString() == "0")
                        Session["IdEntidad"] = lista[0].Id;
                            
                    ViewBag.Entidades = new SelectList(lista, "Id", "Nombre", Session["IdEntidad"]);
                    string idAdminEnt = Session["IdEntidad"].ToString();
                    string nomEnt = rawLista.FirstOrDefault(o => o.Id == idAdminEnt)?.Nombre ?? (ent.Get(idAdminEnt)?.Nombre ?? "");
                    ruta = MicrosoftHelper.MSHelper.GetSiteRoot() + "/Content/images/" + nomEnt + ".jpg";
                    Session["NomEntidad"] = nomEnt;
                }
                else
                {
                    ViewBag.Entidades = new SelectList(lista.Where(o => o.Id == ObjUser.Entidad_Usuario_Id), "Id", "Nombre");
                    Session["IdEntidad"] = ObjUser.Entidad_Usuario_Id;
                    ruta = MicrosoftHelper.MSHelper.GetSiteRoot() + "/Content/images/" + Env.GetUserInfo("logo");
                    string idEnt = Session["IdEntidad"].ToString();
                    Session["NomEntidad"] = rawLista.FirstOrDefault(o => o.Id == idEnt)?.Nombre ?? (ent.Get(idEnt)?.Nombre ?? "");
                }

                string idActual = GetSessionString(FGAConstants.Sesion.IdEntidad, ObjUser?.Entidad_Usuario_Id ?? FGAConstants.Entidades.Administradora);

                if (Session["Periodo"] is null)
                {
                    try
                    {
                        Session["Periodo"] = sp.FGA_Consultar_FechaCierre(idActual).ToShortDateString();
                    }
                    catch
                    {
                        Session["Periodo"] = DateTime.Today.ToShortDateString();
                    }
                }

                DateTime fechaCierre = GetSessionDate("Periodo", DateTime.Today);
                if (Session["Periodo2"] is null)
                    Session["Periodo2"] = fechaCierre.AddMonths(-1).ToShortDateString();

                if (Session["Periodo3"] != null && Session["Periodo2"] != null)
                {
                    try
                    {
                        DateTime f3 = GetSessionDate("Periodo3", fechaCierre);
                        DateTime f2 = GetSessionDate("Periodo2", fechaCierre.AddMonths(-1));
                        if (f3 > f2)
                        {
                            Session["Periodo2"] = f3.ToShortDateString();
                        }
                    }
                    catch { }
                }

                DateTime p2 = GetSessionDate("Periodo2", fechaCierre.AddMonths(-1));
                string tipoComp = GetSessionString("TipoComparacion", FGAConstants.Alertas.ComparacionInteranual);
                Session["TipoComparacion"] = tipoComp;

                if (Session["Periodo1"] is null || (tipoComp.Equals(FGAConstants.Alertas.ComparacionInteranual, StringComparison.OrdinalIgnoreCase) && GetSessionDate("Periodo1", p2.AddYears(-1)) >= p2.AddMonths(-2)))
                {
                    Session["Periodo1"] = p2.AddYears(-1).ToShortDateString();
                }

                Session["Logo"] = ruta;
                if (Session["NomEntidad"] == null)
                {
                    Session["NomEntidad"] = rawLista.FirstOrDefault(o => o.Id == idActual)?.Nombre ?? (ent.Get(idActual)?.Nombre ?? "");
                }
                Session["RutaLogo"] = Server.MapPath("~/Content/images/" + Session["NomEntidad"] + ".jpg");
            }
            catch (Exception)
            {
            }
        }

        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);
        }

        protected override void OnActionExecuting(ActionExecutingContext context)
        {
            if (Response.HeadersWritten)
            {
                base.OnActionExecuting(context);
                return;
            }

            if (Session["IdEntidad"] is null || string.IsNullOrEmpty(Env.GetUserInfo("name")))
            {
                if (!Response.HeadersWritten)
                {
                    HttpCookie c = new HttpCookie(".AspNet.ApplicationCookie")
                    {
                        Expires = DateTime.Now.AddDays(-1)
                    };
                    Response.Cookies.Add(c);

                    HttpCookie d = new HttpCookie("__RequestVerificationToken")
                    {
                        Expires = DateTime.Now.AddDays(-1)
                    };
                    Response.Cookies.Add(d);
                }

                try
                {
                    var AuthenticationManager = HttpContext.GetOwinContext().Authentication;
                    AuthenticationManager.SignOut();
                    Session.Abandon();
                }
                catch (Exception) { }

                context.Result = new RedirectResult("~/Account/Login");
                return;
            }

            base.OnActionExecuting(context);

            try
            {
                int roleid = int.Parse(Env.GetUserInfo("roleid"));
                int userid = int.Parse(Env.GetUserInfo("userid"));
                var descriptor = context.ActionDescriptor;
                var actionName = descriptor.ActionName.ToLower();
                var controllerName = descriptor.ControllerDescriptor.ControllerName.ToLower();
                var GetOrPost = context.HttpContext.Request.HttpMethod.ToString();
                var checkAreaName = context.HttpContext.Request.RequestContext.RouteData.DataTokens["area"];
                string AreaName = "";

                if (checkAreaName != null)
                    AreaName = checkAreaName.ToString().ToLower() + "/";

                var cacheItemKey = "AllMenuBar";
                var globle = HttpRuntime.Cache.Get(cacheItemKey);

                if (Env.GetUserInfo("cambio") == Utility.Utilitarios.Si && actionName != "changepassword")
                {
                    context.Result = new RedirectResult("~/Usuario/ChangePassword");
                    return;
                }

                if (GetOrPost == "POST")
                {
                    if (controllerName == "menupermission" && (actionName == "create" || actionName == "edit" || actionName == "delete" || actionName == "multiviewindex"))
                        globle = MenuBarCache(cacheItemKey, globle, "shortcache");
                }

                if (globle == null)
                {
                    globle = MenuBarCache(cacheItemKey, globle, "60mincache");
                }
                var menuaccess = (MenuOfRole[])globle;
                string menuUrl = AreaName + controllerName + "/" + actionName;

                if (IsActionNameEqualToCrudPageName(actionName))
                    menuUrl = AreaName + controllerName;

                var checkUrl = menuaccess.FirstOrDefault(i => (i.MenuURL == AreaName + controllerName + "/" + actionName) || i.MenuURL == menuUrl);
                if (checkUrl != null)
                {
                    var checkControllerActionRoleUserId = menuaccess.FirstOrDefault(i => (i.MenuURL == menuUrl || i.MenuURL == AreaName + controllerName + "/" + actionName) && i.RoleId == roleid && i.UserId == userid);
                    if (checkControllerActionRoleUserId != null)
                    {
                        if (IsActionNameEqualToCrudPageName(actionName))
                            CheckAccessOfPageAction(context, actionName, checkControllerActionRoleUserId);
                        else
                        {
                            bool isReadRequest = context.HttpContext.Request.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase);
                            if (isReadRequest)
                            {
                                if (checkControllerActionRoleUserId.IsRead == false)
                                    UnAuthoRedirect(context);
                            }
                            else
                            {
                                if (checkControllerActionRoleUserId.IsRead == false || checkControllerActionRoleUserId.IsDelete == false || checkControllerActionRoleUserId.IsCreate == false || checkControllerActionRoleUserId.IsUpdate == false)//if userid !=null && Check Crud
                                    UnAuthoRedirect(context);
                            }
                        }
                    }
                    else
                    {
                        var checkControllerActionRole = menuaccess.FirstOrDefault(i => (i.MenuURL == menuUrl || i.MenuURL == AreaName + controllerName + "/" + actionName) && i.RoleId == roleid && i.UserId == null);
                        if (checkControllerActionRole != null)
                        {
                            if (IsActionNameEqualToCrudPageName(actionName))
                                CheckAccessOfPageAction(context, actionName, checkControllerActionRole);
                            else
                            {
                                bool isReadRequest = context.HttpContext.Request.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase);
                                if (isReadRequest)
                                {
                                    if (checkControllerActionRole.IsRead == false)
                                        UnAuthoRedirect(context);
                                }
                                else
                                {
                                    if (checkControllerActionRole.IsRead == false || checkControllerActionRole.IsDelete == false || checkControllerActionRole.IsCreate == false || checkControllerActionRole.IsUpdate == false)//if userid !=null && Check Crud
                                        UnAuthoRedirect(context);
                                }
                            }
                        }
                        else
                        {
                            if (IsThisAjaxRequest() == false)
                                UnAuthoRedirect(context);
                        }
                    }
                }
            }
            catch (Exception)
            { }
        }

        private bool IsActionNameEqualToCrudPageName(string actionName)
        {
            bool ActionIsCrud;
            switch (actionName)
            {
                case "create":
                case "index":
                case "details":
                case "edit":
                case "multiviewindex":
                case "delete":
                case "consulta":
                case "buscarinformes":
                case "informe":
                case "notificacion":
                case "requisites":
                    ActionIsCrud = true;
                    break;
                default:
                    ActionIsCrud = false;
                    break;
            }

            return ActionIsCrud;
        }

        private void CheckAccessOfPageAction(ActionExecutingContext context, string actionName, MenuOfRole checkRoleUrlCrud)
        {
            switch (actionName)
            {
                case "create":
                    if (checkRoleUrlCrud.IsCreate == false)//Check Crud
                        UnAuthoRedirect(context);
                    break;
                case "index":
                case "details":
                case "consulta":
                case "buscarinformes":
                case "informe":
                case "notificacion":
                case "requisites":
                    if (checkRoleUrlCrud.IsRead == false)//Check Crud
                        UnAuthoRedirect(context);
                    break;
                case "edit":
                case "multiviewindex":
                    if (checkRoleUrlCrud.IsUpdate == false)//Check Crud
                        UnAuthoRedirect(context);
                    break;
                case "delete":
                    if (checkRoleUrlCrud.IsDelete == false)//Check Crud
                        UnAuthoRedirect(context);
                    break;

                default:
                    break;
            }
        }

        private dynamic MenuBarCache(string cacheItemKey, dynamic globle, string cachecaption)
        {
            FGA_En_Linea.MenuPermissionService.MenuPermissionServiceClient db = new FGA_En_Linea.MenuPermissionService.MenuPermissionServiceClient();
            try
            {
                var mp = db.GetAll().Select(m => new MenuOfRole
                {
                    Id = m.Id,
                    MenuURL = m.Menu_MenuId.MenuURL.ToLower(),
                    RoleId = m.RoleId.Value,
                    IsCreate = m.IsCreate,
                    IsDelete = m.IsDelete,
                    IsRead = m.IsRead,
                    IsUpdate = m.IsUpdate
                }).Where(i => i.MenuURL != "root").ToArray();

                globle = mp;
                if (cachecaption == "shortcache")
                    HttpRuntime.Cache.Insert(cacheItemKey, mp, null, DateTime.Now.AddMilliseconds(2), System.Web.Caching.Cache.NoSlidingExpiration);
                else
                    HttpRuntime.Cache.Insert(cacheItemKey, mp, null, DateTime.Now.AddMinutes(60), System.Web.Caching.Cache.NoSlidingExpiration);

                return globle;
            }
            finally
            {
                db.SafeClose();
            }
        }

        private void UnAuthoRedirect(ActionExecutingContext context)
        {
            context.Result = new RedirectResult("~/Account/unauthorized");
        }

        private class MenuOfRole
        {
            public int Id { get; set; }
            public string MenuURL { get; set; }
            public int RoleId { get; set; }
            public Nullable<int> UserId { get; set; }
            public bool IsCreate { get; set; }
            public bool IsRead { get; set; }
            public bool IsUpdate { get; set; }
            public bool IsDelete { get; set; }
        }

        private bool IsThisAjaxRequest()
        {
            bool result = false;
            var currentContext = new HttpContextWrapper(System.Web.HttpContext.Current);
            if (currentContext.Request.Headers["X-Requested-With"] != null && currentContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                result = true;

            return result;
        }

        protected bool IsFGA()
        {
            return Session["IsFGA"] != null && Session["IsFGA"].ToString() == "1";
        }

        #region Helpers Centralizados FFC
        /// <summary>
        /// Cultura institucional costarricense (es-CR) para formateo unificado de montos y fechas
        /// </summary>
        protected CultureInfo CulturaCR => FGAConstants.CulturaCR;

        /// <summary>
        /// Obtiene el identificador de rol actual del usuario en sesión
        /// </summary>
        protected int CurrentRoleId
        {
            get
            {
                int.TryParse(Env.GetUserInfo(FGAConstants.Sesion.RoleId), out int roleId);
                return roleId;
            }
        }

        /// <summary>
        /// Determina si el usuario autenticado posee rol con privilegios de administrador institucional FFC
        /// Si el usuario pertenece a una entidad o cooperativa (su Entidad_Usuario_Id != "0" o Session["IsFGA"] == 0),
        /// SIEMPRE retorna false para garantizar aislamiento estricto de datos.
        /// </summary>
        protected bool IsCurrentUserAdmin
        {
            get
            {
                // 1. Si Session["IsFGA"] indica explícitamente 0, es usuario de entidad
                if (Session != null && Session["IsFGA"] != null && Session["IsFGA"].ToString() == "0")
                {
                    return false;
                }

                // 2. Si el claim "EsEntidad" indica que es entidad
                string esEntidadClaim = Env.GetUserInfo("esentidad");
                if (esEntidadClaim == "1")
                {
                    return false;
                }

                // 3. Si el usuario logueado tiene una Entidad_Usuario_Id asignada diferente de "0" (Administradora FFC)
                Usuario usrObj = Session?["CurrentUserObj"] as Usuario;
                if (usrObj == null && Session != null)
                {
                    try
                    {
                        string uid = Env.GetUserInfo("userid");
                        if (!string.IsNullOrEmpty(uid))
                        {
                            usrObj = usr.Get(uid);
                            Session["CurrentUserObj"] = usrObj;
                        }
                    }
                    catch { }
                }

                if (usrObj != null && !string.IsNullOrEmpty(usrObj.Entidad_Usuario_Id) &&
                    usrObj.Entidad_Usuario_Id != Utility.Utilitarios.entidadAdministradora)
                {
                    return false;
                }

                int roleId = CurrentRoleId;
                if (roleId != 0 && !FGAConstants.Roles.EsAdministrador(roleId))
                {
                    return false;
                }

                return IsFGA() || FGAConstants.Roles.EsAdministrador(roleId);
            }
        }

        /// <summary>
        /// Resuelve la entidad contable activa para el usuario y contexto actual.
        /// Si el usuario no es administrador institucional FFC, FUERZA de manera inmutable la entidad asignada a su perfil.
        /// </summary>
        protected string ResolveCurrentEntity(string entidadParam, out bool esAdmin)
        {
            esAdmin = IsCurrentUserAdmin;

            // Obtener el ID de entidad del usuario
            string idEntidadUsuario = null;
            Usuario usrObj = Session?["CurrentUserObj"] as Usuario;
            if (usrObj == null && Session != null)
            {
                try
                {
                    string uid = Env.GetUserInfo("userid");
                    if (!string.IsNullOrEmpty(uid))
                    {
                        usrObj = usr.Get(uid);
                        Session["CurrentUserObj"] = usrObj;
                    }
                }
                catch { }
            }

            if (usrObj != null && !string.IsNullOrEmpty(usrObj.Entidad_Usuario_Id))
            {
                idEntidadUsuario = usrObj.Entidad_Usuario_Id;
            }
            if (string.IsNullOrEmpty(idEntidadUsuario))
            {
                idEntidadUsuario = Env.GetUserInfo("identidad");
            }
            if (string.IsNullOrEmpty(idEntidadUsuario) && Session != null && Session["IdEntidad"] != null)
            {
                idEntidadUsuario = Session["IdEntidad"].ToString();
            }

            // AISLAMIENTO MULTI-TENANT ESTRICTO:
            // Si el usuario no es administrador, NUNCA puede consultar otra entidad ni "TODAS"
            if (!esAdmin)
            {
                string idForzada = (!string.IsNullOrEmpty(idEntidadUsuario) && idEntidadUsuario != FGAConstants.Entidades.Todas && idEntidadUsuario != FGAConstants.Entidades.Administradora)
                    ? idEntidadUsuario
                    : (Session?[FGAConstants.Sesion.IdEntidad]?.ToString() ?? "2");

                if (Session != null)
                {
                    Session[FGAConstants.Sesion.IdEntidad] = idForzada;
                }
                return idForzada;
            }

            // Administrador institucional FFC: permite alternar entre "TODAS" (-1) o una entidad específica
            string idEntidadSession = Session?[FGAConstants.Sesion.IdEntidad]?.ToString();
            string idEntidad = string.IsNullOrEmpty(entidadParam)
                ? (string.IsNullOrEmpty(idEntidadSession) ? FGAConstants.Entidades.Todas : idEntidadSession)
                : entidadParam;

            if (Session != null)
            {
                Session[FGAConstants.Sesion.IdEntidad] = idEntidad;
            }

            return idEntidad;
        }

        /// <summary>
        /// Retorna la lista activa de entidades/cooperativas para combos y filtros, con almacenamiento en caché en memoria
        /// </summary>
        protected List<Entidad> GetEntidadesCombo()
        {
            try
            {
                var cached = System.Web.HttpContext.Current?.Cache["CatEntidadesCombo"] as List<Entidad>;
                if (cached != null && cached.Count > 0)
                {
                    return cached;
                }

                var entClient = new FGA_En_Linea.EntidadService.EntidadServiceClient();
                try
                {
                    var entidades = (entClient.GetAll() ?? new Entidad[0])
                        .Where(e => e != null && !string.IsNullOrWhiteSpace(e.Nombre) &&
                                    e.Id != FGAConstants.Entidades.Todas &&
                                    e.Id != FGAConstants.Entidades.Administradora &&
                                    e.Id != FGAConstants.Entidades.Comodin &&
                                    !e.Nombre.Trim().Equals(FGAConstants.Entidades.TextoTodasCooperativas, StringComparison.OrdinalIgnoreCase) &&
                                    !e.Nombre.Trim().Equals(FGAConstants.Entidades.TextoTodasEntidades, StringComparison.OrdinalIgnoreCase))
                        .OrderBy(e => e.Nombre)
                        .ToList();

                    if (entidades.Count > 0 && System.Web.HttpContext.Current != null)
                    {
                        System.Web.HttpContext.Current.Cache.Insert("CatEntidadesCombo", entidades, null, DateTime.Now.AddMinutes(15), System.Web.Caching.Cache.NoSlidingExpiration);
                    }
                    return entidades;
                }
                finally
                {
                    entClient.SafeClose();
                }
            }
            catch
            {
                return new List<Entidad>();
            }
        }

        /// <summary>
        /// Configura el SelectList de ViewBag.Entidades respetando permisos de Administrador vs Entidad individual
        /// </summary>
        protected void ConfigurarComboEntidades(string selectedId = null, bool incluirTodas = true)
        {
            try
            {
                Usuario ObjUser = Session[FGAConstants.Sesion.CurrentUserObj] as Usuario;
                if (ObjUser == null)
                {
                    ObjUser = usr.Get(Env.GetUserInfo(FGAConstants.Sesion.UserId));
                    Session[FGAConstants.Sesion.CurrentUserObj] = ObjUser;
                }

                if (IsCurrentUserAdmin || (ObjUser != null && ObjUser.Entidad_Usuario_Id == FGAConstants.Entidades.Administradora))
                {
                    var lista = new List<Entidad>(GetEntidadesCombo());
                    if (incluirTodas)
                    {
                        lista.Add(new Entidad
                        {
                            Id = FGAConstants.Entidades.Todas,
                            Nombre = FGAConstants.Entidades.TextoTodasCooperativas
                        });
                    }
                    ViewBag.Entidades = new SelectList(lista, "Id", "Nombre", selectedId ?? Session[FGAConstants.Sesion.IdEntidad]);
                }
                else if (ObjUser != null)
                {
                    var lista = ent.GetAll().Where(o => o.Id == ObjUser.Entidad_Usuario_Id).ToList();
                    ViewBag.Entidades = new SelectList(lista, "Id", "Nombre", selectedId ?? ObjUser.Entidad_Usuario_Id);
                }
            }
            catch
            {
                ViewBag.Entidades = new SelectList(new List<Entidad>(), "Id", "Nombre");
            }
        }

        /// <summary>
        /// Obtiene la lista de parámetros del sistema desde caché en memoria (30 min) o BD mediante WCF.
        /// </summary>
        public static List<FGA.Models.Parametros> GetParametrosSistema()
        {
            const string cacheKey = "Cache_Parametros_Sistema";
            var cached = System.Web.HttpContext.Current?.Cache[cacheKey] as List<FGA.Models.Parametros>;
            if (cached != null) return cached;

            var paramClient = new FGA_En_Linea.ParametrosService.ParametrosServiceClient();
            try
            {
                var raw = paramClient.GetAll();
                var list = raw != null ? raw.ToList() : new List<FGA.Models.Parametros>();
                if (System.Web.HttpContext.Current != null && list.Count > 0)
                {
                    System.Web.HttpContext.Current.Cache.Insert(
                        cacheKey,
                        list,
                        null,
                        DateTime.Now.AddMinutes(30),
                        System.Web.Caching.Cache.NoSlidingExpiration);
                }
                paramClient.SafeClose();
                return list;
            }
            catch
            {
                paramClient.SafeClose();
                return new List<FGA.Models.Parametros>();
            }
        }

        /// <summary>
        /// Obtiene el valor de un parámetro del sistema por su llave.
        /// </summary>
        public static string GetValorParametro(string llave, string fallback = "")
        {
            var list = GetParametrosSistema();
            var item = list.FirstOrDefault(p => string.Equals(p.Llave, llave, StringComparison.OrdinalIgnoreCase));
            return item != null && !string.IsNullOrEmpty(item.Valor) ? item.Valor : fallback;
        }
        #endregion

        #region Session & Date Helpers
        /// <summary>
        /// Obtiene una fecha de Session de forma segura evitando NullReferenceException o FormatException.
        /// </summary>
        protected DateTime GetSessionDate(string key, DateTime? fallback = null)
        {
            DateTime def = fallback ?? DateTime.Today;
            object val = Session != null ? Session[key] : null;
            return Utilitarios.ConvertirAFechaSegura(val, def);
        }

        /// <summary>
        /// Asigna una fecha a la sesión formateada como fecha corta estandarizada.
        /// </summary>
        protected void SetSessionDate(string key, DateTime date)
        {
            if (Session != null)
            {
                Session[key] = date.ToShortDateString();
            }
        }

        /// <summary>
        /// Obtiene un string de Session de forma segura.
        /// </summary>
        protected string GetSessionString(string key, string fallback = "")
        {
            if (Session == null || Session[key] == null) return fallback;
            return Session[key].ToString();
        }

        /// <summary>
        /// Obtiene un entero de Session de forma segura.
        /// </summary>
        protected int GetSessionInt(string key, int fallback = 0)
        {
            if (Session == null || Session[key] == null) return fallback;
            int res;
            if (int.TryParse(Session[key].ToString(), out res))
                return res;
            return fallback;
        }
        #endregion

        private readonly FGA_En_Linea.EntidadService.EntidadServiceClient ent = new FGA_En_Linea.EntidadService.EntidadServiceClient();
        private readonly FGA_En_Linea.UsuarioService.UsuarioServiceClient usr = new FGA_En_Linea.UsuarioService.UsuarioServiceClient();
        private readonly FGA_En_Linea.SPService.SPClient sp = new FGA_En_Linea.SPService.SPClient();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ent.SafeClose();
                usr.SafeClose();
                sp.SafeClose();
            }
            base.Dispose(disposing);
        }
    }
}
