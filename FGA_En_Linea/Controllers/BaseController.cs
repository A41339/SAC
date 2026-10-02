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
                    string idActual = Session["IdEntidad"].ToString();
                    string nomEnt = rawLista.FirstOrDefault(o => o.Id == idActual)?.Nombre ?? (ent.Get(idActual)?.Nombre ?? "");
                    ruta = MicrosoftHelper.MSHelper.GetSiteRoot() + "/Content/images/" + nomEnt + ".jpg";
                    Session["NomEntidad"] = nomEnt;
                }
                else
                {
                    ViewBag.Entidades = new SelectList(lista.Where(o => o.Id == ObjUser.Entidad_Usuario_Id), "Id", "Nombre");
                    Session["IdEntidad"] = ObjUser.Entidad_Usuario_Id;
                    ruta = MicrosoftHelper.MSHelper.GetSiteRoot() + "/Content/images/" + Env.GetUserInfo("logo");
                    string idActual = Session["IdEntidad"].ToString();
                    Session["NomEntidad"] = rawLista.FirstOrDefault(o => o.Id == idActual)?.Nombre ?? (ent.Get(idActual)?.Nombre ?? "");
                }

                if (Session["Periodo"] is null)
                    Session["Periodo"] = sp.FGA_Consultar_FechaCierre(Session["IdEntidad"].ToString()).ToShortDateString();

                DateTime fechaCierre = Utility.Utilitarios.ConvertirAFecha(Session["Periodo"].ToString());
                if (Session["Periodo2"] is null)
                    Session["Periodo2"] = fechaCierre.AddMonths(-1).ToShortDateString();

                if (Session["Periodo3"] != null && Session["Periodo2"] != null)
                {
                    try
                    {
                        DateTime f3 = Utility.Utilitarios.ConvertirAFecha(Session["Periodo3"].ToString());
                        DateTime f2 = Utility.Utilitarios.ConvertirAFecha(Session["Periodo2"].ToString());
                        if (f3 > f2)
                        {
                            Session["Periodo2"] = f3.ToShortDateString();
                        }
                    }
                    catch { }
                }

                DateTime p2 = Utility.Utilitarios.ConvertirAFecha(Session["Periodo2"].ToString());
                string tipoComp = Session["TipoComparacion"]?.ToString() ?? "Interanual";
                if (Session["TipoComparacion"] is null)
                    Session["TipoComparacion"] = "Interanual";

                if (Session["Periodo1"] is null || (tipoComp.Equals("Interanual", StringComparison.OrdinalIgnoreCase) && Utility.Utilitarios.ConvertirAFecha(Session["Periodo1"].ToString()) >= p2.AddMonths(-2)))
                {
                    Session["Periodo1"] = p2.AddYears(-1).ToShortDateString();
                }

                Session["Logo"] = ruta;
                if (Session["NomEntidad"] == null)
                {
                    string idActual = Session["IdEntidad"].ToString();
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

               // if (globle == null)//if cashe is null
                globle = MenuBarCache(cacheItemKey, globle, "60mincache");//make cache from db
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
        /// Determina si el usuario autenticado posee rol con privilegios de administrador institucional
        /// </summary>
        protected bool IsCurrentUserAdmin => FGAConstants.Roles.EsAdministrador(CurrentRoleId);

        /// <summary>
        /// Resuelve la entidad contable activa para el usuario y contexto actual.
        /// Si el usuario no es administrador, fuerza la entidad asignada a su perfil.
        /// </summary>
        protected string ResolveCurrentEntity(string entidadParam, out bool esAdmin)
        {
            esAdmin = IsCurrentUserAdmin;
            string idEntidadSession = Session?[FGAConstants.Sesion.IdEntidad]?.ToString();
            string idEntidadUsuario = Env.GetUserInfo(FGAConstants.Sesion.Entidad);

            string idEntidad = string.IsNullOrEmpty(entidadParam)
                ? (string.IsNullOrEmpty(idEntidadSession) ? FGAConstants.Entidades.Todas : idEntidadSession)
                : entidadParam;

            if (!esAdmin && !string.IsNullOrEmpty(idEntidadUsuario) &&
                idEntidadUsuario != FGAConstants.Entidades.Todas &&
                idEntidadUsuario != FGAConstants.Entidades.Administradora)
            {
                idEntidad = idEntidadUsuario;
            }

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

                using (var entClient = new FGA_En_Linea.EntidadService.EntidadServiceClient())
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
        #endregion

        private readonly FGA_En_Linea.EntidadService.EntidadServiceClient ent = new FGA_En_Linea.EntidadService.EntidadServiceClient();
        private readonly FGA_En_Linea.UsuarioService.UsuarioServiceClient usr = new FGA_En_Linea.UsuarioService.UsuarioServiceClient();
        private readonly FGA_En_Linea.SPService.SPClient sp = new FGA_En_Linea.SPService.SPClient();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ent.Close();
                usr.Close();
                sp.Close();
            }
            base.Dispose(disposing);
        }
    }
}
