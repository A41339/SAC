using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using FGA.Models;
using FGA.Utility;
using System.Collections.Generic;

namespace SignalRChat
{
    public class ChatHub : Hub
    {
        static ConcurrentDictionary<string, ChatData.User> _userConnections = new ConcurrentDictionary<string, ChatData.User>();

        private static List<ChatData.User> GetContactById(string id)
        {
            List<ChatData.User> contacts = new List<ChatData.User>();
            if (_userConnections.ContainsKey(id))
                contacts.Add(_userConnections.Where(u => u.Value.Id.Equals(id, StringComparison.OrdinalIgnoreCase)).FirstOrDefault().Value);

            if (id == Utilitarios.entidadAdministradora)
                foreach (var contact in _userConnections)
                    if (contact.Value.Name == Utilitarios.nombreEntidadAdmin)
                        contacts.Add(contact.Value);

            return contacts;
        }

        public void Send(string idUser, string idContact, string message)
        {
            string time = DateTime.Now.ToString();
            var users = GetContactById(idUser);
            var user = users.FirstOrDefault();
            idContact = string.IsNullOrEmpty(idContact) ? Utilitarios.entidadAdministradora : idContact;

            //Si el FGA no ha seleccionado con quien desea conversar mostrar una advertencia
            if (user.Company.Equals(Utilitarios.nombreEntidadAdmin) && idContact.Equals(Utilitarios.entidadAdministradora))
            {
                Clients.Client(user.ConnectionId).addNewMessageToPage(Utilitarios.nombreEntidadAdmin, Utilitarios.nombreEntidadAdmin,
                    "Debe seleccionar el usuario con el que desee hablar", time, idContact, idUser);
                return;
            }

            var contacts = GetContactById(idContact);
            if (contacts.Count() <= 0)
                Clients.Client(user.ConnectionId).addNewMessageToPage(Utilitarios.nombreEntidadAdmin, Utilitarios.nombreEntidadAdmin,
                    "No hay operadores en este momento. Disculpe las molestias", time, idContact, idUser);
            else
            {
                var userName = users.FirstOrDefault().Name;
                var contactName = contacts.FirstOrDefault().Name;

                foreach (var contact in contacts)
                    if (contact?.ConnectionId != null)
                        Clients.Client(contact.ConnectionId).addNewMessageToPage(userName, contact.Name, message, time,
                                user.Company.Equals(Utilitarios.nombreEntidadAdmin) ? Utilitarios.entidadAdministradora : idUser);

                foreach (var detail in users)
                    Clients.Client(detail.ConnectionId).addNewMessageToPage(detail.Name, contactName, message, time, idContact, idUser);

                ChatData.AddMessageToHistory(userName, contactName, message);
            }
        }

        public async Task GetHistory(string idUser, string idContact, int unreadMessages)
        {
            try
            {
                var users = GetContactById(idUser);
                var contactName = string.IsNullOrEmpty(idContact) ? Utilitarios.nombreEntidadAdmin : GetContactById(idContact).FirstOrDefault().Name;
                List<Message> history = await ChatData.GetMessageHistory(users.FirstOrDefault().Name, contactName, unreadMessages);

                foreach (var message in history)
                {
                    Clients.Client(users.FirstOrDefault().ConnectionId).addNewMessageToPage(message.FromUser, message.ToUser, message.Content, message.Time, idContact,
                        message.ToUser == contactName ? idUser : idContact);
                }
            }
            catch (Exception)
            { }
        }

        public override Task OnConnected()
        {
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            try
            {
                var user = _userConnections.Where(o => o.Value.ConnectionId == Context.ConnectionId).FirstOrDefault();
                var remove = user.Value;
                _userConnections.TryRemove(user.Key, out remove);
            }
            catch (Exception)
            { }

            return base.OnDisconnected(stopCalled);
        }

        public void Connect(string idUser, string nameUser, string idContact, string companyName)
        {
            var pair = _userConnections.FirstOrDefault(u => u.Value.Id == idUser);
            if (pair.Value != null)
                if (!_userConnections.TryRemove(pair.Key, out ChatData.User value))
                    return;

            var newUser = new ChatData.User()
            {
                Id = idUser,
                Name = companyName.Contains(Utilitarios.nombreEntidadAdmin) ? Utilitarios.nombreEntidadAdmin : System.Web.HttpUtility.HtmlDecode(nameUser),
                Company = companyName,
                Logo = MicrosoftHelper.MSHelper.GetSiteRoot() + "/Content/images/" + companyName + ".jpg",
                ConnectionId = Context.ConnectionId,
            };

            Clients.Others.clearContacts();

            if (!_userConnections.TryAdd(idUser, newUser))
                return;

            foreach (var u in _userConnections.Values)
            {
                if (u.Id != idUser)
                    Clients.Caller.addContact(u.Id, u.Name, u.Company, u.Logo);
                else
                    Clients.Others.addContact(u.Id, u.Name, u.Company, u.Logo);
            }
        }
    }
}