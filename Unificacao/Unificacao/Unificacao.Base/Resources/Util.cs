using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furnas.GestaoSPE.Unificacao.Base.Resources
{
    public static class Util
    {
        public static string ValidaTextField(object fieldvalue)
        {
            return (fieldvalue != null) ? Convert.ToString(fieldvalue) : string.Empty;
        }

        public static void Log(string source, TraceSeverity traceSeverity, EventSeverity eventSeverity, string logMessage)
        {
            try
            {
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(source, traceSeverity, eventSeverity), traceSeverity, logMessage, null);
            }
            catch (Exception)
            {
                // maybe write to Event Log3?
            }
        }

        public static string CreateCAMLQuery(List<string> parameters, string orAndCondition, bool isIncludeWhereClause)
        {
            StringBuilder sb = new StringBuilder();
            if (parameters.Count == 0)
            {
                AppendEQ(sb, "all");
            }
            int j = 0;
            for (int i = 0; i < parameters.Count; i++)
            {
                if (!string.IsNullOrEmpty(parameters[i].Split(';')[3]))
                {
                    AppendEQ(sb, parameters[i]);

                    if (i > 0 && j > 0)
                    {
                        sb.Insert(0, "<" + orAndCondition + ">");
                        sb.Append("</" + orAndCondition + ">");
                    }
                    j++;
                }
            }
            if (isIncludeWhereClause)
            {
                sb.Insert(0, "<Where>");
                sb.Append("</Where>");
            }
            return sb.ToString();
        }

        public static void AppendEQ(StringBuilder sb, string value)
        {
            string[] field = value.Split(';');
            sb.AppendFormat("<{0}>", field[2].ToString());
            sb.AppendFormat("<FieldRef Name='{0}'/>", field[0].ToString());
            sb.AppendFormat("<Value Type='{0}'>{1}</Value>", field[1].ToString(), field[3].ToString());
            sb.AppendFormat("</{0}>", field[2].ToString());
        }

        public static string CreateCAMLQuery(List<string> parameters, string orAndCondition, bool isIncludeWhereClause, bool islookup)
        {
            StringBuilder sb = new StringBuilder();
            if (parameters.Count == 0)
            {
                AppendEQ(sb, "all", islookup);
            }
            int j = 0;
            for (int i = 0; i < parameters.Count; i++)
            {
                if (!string.IsNullOrEmpty(parameters[i].Split(';')[3]))
                {
                    AppendEQ(sb, parameters[i], islookup);

                    if (i > 0 && j > 0)
                    {
                        sb.Insert(0, "<" + orAndCondition + ">");
                        sb.Append("</" + orAndCondition + ">");
                    }
                    j++;
                }
            }
            if (isIncludeWhereClause)
            {
                sb.Insert(0, "<Where>");
                sb.Append("</Where>");
            }
            return sb.ToString();
        }

        public static void AppendEQ(StringBuilder sb, string value, bool isLookup)
        {
            string[] field = value.Split(';');
            sb.AppendFormat("<{0}>", field[2].ToString());
            if (isLookup)
                sb.AppendFormat("<FieldRef Name='{0}' LookupId='true' />", field[0].ToString());
            else
                sb.AppendFormat("<FieldRef Name='{0}'/>", field[0].ToString());

            sb.AppendFormat("<Value Type='{0}'>{1}</Value>", field[1].ToString(), field[3].ToString());
            sb.AppendFormat("</{0}>", field[2].ToString());
        }

        /// <summary>
        /// Grava log na lista Logs. Vou colocar para gravar no site configurações na lista Logs que terá lá.
        /// </summary>
        /// <param name="logErros"></param>
        /// <param name="web"></param>
        /// <param name="title">Título do log</param>
        public static void GravarLogs(List<string> logErros, SPWeb web, string title)
        {
            if (logErros.Count > 0)
            {
                SPList list = web.Lists["Logs"];

                foreach (string erro in logErros)
                {
                    SPListItem item = list.Items.Add();
                    item[SPBuiltInFieldId.Title] = title;
                    item["Erro"] = erro;
                    item.Update();
                }

            }
        }

        public static void GravarLogs(string logErros, SPWeb web, string title)
        {
            SPList list = web.Lists["Logs"];

            SPListItem item = list.Items.Add();
            item[SPBuiltInFieldId.Title] = title;
            item["Descricao"] = logErros;
            item.Update();

        }
    }
}
