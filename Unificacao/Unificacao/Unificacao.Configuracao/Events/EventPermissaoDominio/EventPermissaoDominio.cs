using System;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.Workflow;
using Furnas.GestaoSPE.Unificacao.Base.Service;
using Furnas.GestaoSPE.Unificacao.Base.Resources;

namespace Furnas.GestaoSPE.Unificacao.Configuracao.Events.EventPermissaoDominio
{
    /// <summary>
    /// List Item Events
    /// </summary>
    public class EventPermissaoDominio : SPItemEventReceiver
    {
        /// <summary>
        /// An item is being added.
        /// </summary>
        public override void ItemAdding(SPItemEventProperties properties)
        {
            base.ItemAdding(properties);

            try
            {
                ServicePermissaoDominio dominio = new ServicePermissaoDominio(properties);
                dominio.AtribuirPermissao();
            }
            catch (Exception err)
            {
                Util.GravarLogs(err.Message, properties.Web, "Permissão por dominio - Add");
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
        }

        /// <summary>
        /// An item is being deleted.
        /// </summary>
        public override void ItemDeleting(SPItemEventProperties properties)
        {
            base.ItemDeleting(properties);

            try
            {
                ServicePermissaoDominio dominio = new ServicePermissaoDominio(properties);
                dominio.RemoverPermissao();
            }
            catch (Exception err)
            {
                Util.GravarLogs(err.Message, properties.Web, "Permissão por dominio - Add");
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
            try
            {
                ServicePermissaoDominio dominio = new ServicePermissaoDominio(properties);
                dominio.AtualizarPermissoes(properties);
            }
            catch (Exception err)
            {
                Util.GravarLogs(err.Message, properties.Web, "Permissão por dominio - Update");
                properties.ErrorMessage = err.Message;
                properties.Status = SPEventReceiverStatus.Continue;
            }
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