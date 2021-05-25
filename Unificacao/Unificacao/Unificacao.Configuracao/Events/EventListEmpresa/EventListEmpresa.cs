using System;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.Workflow;
using Furnas.GestaoSPE.Unificacao.Base.Infrastruture;
using Furnas.GestaoSPE.Unificacao.Base.Service;

namespace Furnas.GestaoSPE.Unificacao.Configuracao.Events.EventListEmpresa
{
    /// <summary>
    /// List Item Events
    /// </summary>
    public class EventListEmpresa : SPItemEventReceiver
    {
        /// <summary>
        /// An item is being added.
        /// </summary>
        public override void ItemAdding(SPItemEventProperties properties)
        {
            base.ItemAdding(properties);
            try
            {
                if (!properties.Web.CurrentUser.IsSiteAdmin)
                    throw new Exception("Somente o administrador do sistema pode adicionar uma empresa");
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
                if (!properties.Web.CurrentUser.IsSiteAdmin)
                    throw new Exception("Somente o administrador do sistema pode alterar uma empresa.");
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