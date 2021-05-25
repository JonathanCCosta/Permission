using System;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.Workflow;
using Furnas.GestaoSPE.Unificacao.Base.Service;
using Furnas.GestaoSPE.Unificacao.Base.Infrastruture;

namespace Furnas.GestaoSPE.Unificacao.Configuracao.Events.EventGruposEspecificos
{
    /// <summary>
    /// List Item Events
    /// </summary>
    public class EventGruposEspecificos : SPItemEventReceiver
    {
        ServicoGrupoEspecifico GrupoEspecifico = null;
        /// <summary>
        /// An item is being added.
        /// </summary>
        public override void ItemAdding(SPItemEventProperties properties)
        {
            base.ItemAdding(properties);
            try
            {
                //if (!properties.Web.CurrentUser.IsSiteAdmin)
                //    throw new Exception("Somente o administrador do sistema pode adicionar um grupo específico no sistema");

                string grupo = Convert.ToString(properties.AfterProperties[SPEncode.UrlDecodeAsUrl("Title")]);
                GrupoEspecifico = new ServicoGrupoEspecifico(grupo, properties.Web);

                if (!GrupoEspecifico.ExisteGrupoEspecifico(properties.Web.SiteGroups, grupo))
                {
                    Seguranca.AddGruposEspecificos(properties.Web.ParentWeb, grupo);
                }
                else
                {
                    throw new Exception("O nome especificado já está sendo usado. Tente novamente com outro nome.");
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
                GrupoEspecifico = new ServicoGrupoEspecifico(properties);

                string titulo = properties.ListItem.Title;
                string tituloAfterProperties = Convert.ToString(properties.AfterProperties[SPEncode.UrlDecodeAsUrl("Title")]);

                if (titulo != tituloAfterProperties)
                {
                    if (!GrupoEspecifico.ExisteGrupoEspecifico(properties.Web.SiteGroups, tituloAfterProperties))
                    {
                        base.EventFiringEnabled = false;
                        Seguranca.AddGruposEspecificos(properties.Web.ParentWeb, tituloAfterProperties);
                        GrupoEspecifico.AtualizaUsuarios(properties);
                        Seguranca.RemoverGrupo(properties.Web.ParentWeb, titulo);
                        base.EventFiringEnabled = true;
                    }
                    else
                    {
                        throw new Exception("O nome especificado já está sendo usado. Tente novamente com outro nome.");
                    }
                }
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
                //if (!properties.Web.CurrentUser.IsSiteAdmin)
                //    throw new Exception("Somente o administrador do sistema pode adicionar um grupo de acesso");
                
                string grupo = properties.ListItem[SPBuiltInFieldId.Title].ToString(); 
                GrupoEspecifico = new ServicoGrupoEspecifico(grupo, properties.Web);

                Seguranca.RemoverGrupo(properties.Web.ParentWeb, grupo);
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