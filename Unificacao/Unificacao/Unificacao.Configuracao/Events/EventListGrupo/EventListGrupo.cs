using System;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.Workflow;
using Furnas.GestaoSPE.Unificacao.Base.Service;
using Furnas.GestaoSPE.Unificacao.Base.Infrastruture;

namespace Furnas.GestaoSPE.Unificacao.Configuracao.Events.EventListGrupo
{
    /// <summary>
    /// List Item Events
    /// </summary>
    public class EventListGrupo : SPItemEventReceiver
    {
        ServiceGrupo Grupo = null;

        /// <summary>
        /// An item is being added.
        /// </summary>
        public override void ItemAdding(SPItemEventProperties properties)
        {
            base.ItemAdding(properties);
            try
            {
                if (!properties.Web.CurrentUser.IsSiteAdmin)
                    throw new Exception("Somente o administrador do sistema pode adicionar um grupo de acesso");

                Grupo = new ServiceGrupo(properties);

                if (!Grupo.GrupoExiste(properties))
                {
                    SPRoleType role = Grupo.Perfil();
                    string grupo = Grupo._NomeGrupo == null ? Grupo._NomeGrupoAfterProperties : Grupo._NomeGrupo;
                    Seguranca.AdicionarGrupo(properties.Web.ParentWeb, role, grupo);
                }

            }
            catch (Exception err)
            {
                properties.ErrorMessage = err.Message;
                properties.Status = SPEventReceiverStatus.CancelWithError;
            }
        }

        /// <summary>
        /// An item is being updated.
        /// </summary>
        public override void ItemUpdating(SPItemEventProperties properties)
        {
            base.ItemUpdating(properties);
            try
            {
                Grupo = new ServiceGrupo(properties);

                string titulo = properties.ListItem.Title;
                string tituloAfterProperties = Convert.ToString(properties.AfterProperties[SPEncode.UrlDecodeAsUrl("Title")]);

                string perfil = new SPFieldLookupValue(Convert.ToString(properties.ListItem["Perfil"])).LookupValue;
                string perfilAfterProperties = Grupo._Perfil;

                string empresa = new SPFieldLookupValue(Convert.ToString(properties.ListItem["Empresa"])).LookupValue;
                string empresaAfterProperties = Grupo._Empresa;


                if (string.Compare(titulo, tituloAfterProperties, true) != 0
                    || string.Compare(perfil, perfilAfterProperties, true) != 0
                    || string.Compare(empresa, empresaAfterProperties, true) != 0)
                {
                    if (!properties.Web.CurrentUser.IsSiteAdmin)
                        throw new Exception("Somente o administrador do sistema pode alterar a url dos documentos.");
                }

                Grupo.AtualizaUsuarios(properties);
                //Empresa = new ServiceEmpresa(properties);
                //Empresa.AtualizaUsuarios(properties);
            }
            catch (Exception err)
            {
                properties.ErrorMessage = err.Message;
                properties.Status = SPEventReceiverStatus.CancelWithError;
            }
        }

        /// <summary>
        /// An item is being deleted.
        /// </summary>
        public override void ItemDeleting(SPItemEventProperties properties)
        {
            base.ItemDeleting(properties);

            try
            {
                if (!properties.Web.CurrentUser.IsSiteAdmin)
                    throw new Exception("Somente o administrador do sistema pode adicionar um grupo de acesso");


                Grupo = new ServiceGrupo(properties);

                Seguranca.RemoverGrupo(properties.Web.ParentWeb, Grupo._NomeGrupo);
            }
            catch (Exception err)
            {
                properties.ErrorMessage = err.Message;
                properties.Status = SPEventReceiverStatus.CancelWithError;
            }
        }

        /// <summary>
        /// An item was added.
        /// </summary>
        public override void ItemAdded(SPItemEventProperties properties)
        {
            base.ItemAdded(properties);
            try
            {
                Grupo = new ServiceGrupo(properties);
                Grupo.AddUsuarios(properties.ListItem);
            }
            catch (Exception err)
            {
                properties.ErrorMessage = err.Message;
                properties.Status = SPEventReceiverStatus.CancelWithError;
            }
        }

        /// <summary>
        /// An item was updated.
        /// </summary>
        public override void ItemUpdated(SPItemEventProperties properties)
        {
            base.ItemUpdated(properties);
        }

        /// <summary>
        /// An item was deleted.
        /// </summary>
        public override void ItemDeleted(SPItemEventProperties properties)
        {
            base.ItemDeleted(properties);
        }


    }
}