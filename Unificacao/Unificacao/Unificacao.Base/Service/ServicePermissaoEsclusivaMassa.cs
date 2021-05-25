using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Furnas.GestaoSPE.Unificacao.Base.Resources;

namespace Furnas.GestaoSPE.Unificacao.Base.Service
{
    public static class ServicePermissaoEsclusivaMassa
    {
        /// <summary>
        /// Garante permissão exclusiva para todos os items existente de Furnas.
        /// </summary>
        /// <param name="web"></param>
        public static void ExclusivePermissionInLargeScale(SPWeb web)
        {
            try
            {
                SPListItemCollection collPermissionExlusive = GetListPermissionExclusive(web);
                SPList list;
                DataTable tableGrupos = ObterGrupos("Furnas", web);
                //Varre a lista de Permissões Exclusivas para obter o nome das lista onde os itens terão permissões exclusivas
                foreach (SPListItem itemList in collPermissionExlusive)
                {
                    list = web.Lists.TryGetList(itemList.Title);
                    if (list != null)
                    {
                        foreach (SPListItem item in list.Items)
                        {
                            try
                            {
                                SetPermissionExclusive(item, tableGrupos, web);
                            }
                            catch (Exception err)
                            {
                                WriteLog(web, "Permissão em Item - Lista: " + list.Title + " ID do Item: " + item.ID, err.Message);
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                WriteLog(web, "Permissão em Massa", err.Message);
            }
        }

        /// <summary>
        /// Garante permissão exclusiva para os documentos de Furnas
        /// </summary>
        /// <param name="web"></param>
        public static void ExclusivePermissionInLargeScaleItems(SPWeb web)
        {
            try
            {
                DataTable tableGrupos = ObterGrupos("FurnasDocs", web);
                    SPList list = web.Lists.TryGetList(Constants.NameLibraryLegado.ListSPE);
                    if (list != null)
                    {
                        foreach (SPListItem item in list.Items)
                        {
                            try
                            {
                                SetPermissionExclusive(item, tableGrupos, web);
                            }
                            catch (Exception err)
                            {
                                WriteLog(web, "Permissão em Item - Lista: " + list.Title + " ID do Item: " + item.ID, err.Message);
                            }
                        }
                    }
            }
            catch (Exception err)
            {
                WriteLog(web, "Permissão em Massa", err.Message);
            }
        }

        public static void WriteLog(SPWeb web, string titulo, string erro)
        {
            SPList list = web.Webs["configuracoes"].Lists.TryGetList("Logs");
            if (list != null)
            {
                SPListItem item = list.AddItem();
                item[SPBuiltInFieldId.Title] = titulo;
                item["Descricao"] = erro;
                item.Update();
            }
        }

        public static void SetPermissionExclusive(SPListItem item, DataTable tableGrupos, SPWeb web)
        {
            if (tableGrupos != null)
            {
                //Quebra a herança
                if (!item.HasUniqueRoleAssignments)
                {
                    item.BreakRoleInheritance(false, true);
                }

                //Remove todas as permissões
                SPRoleAssignmentCollection SPRoleAssColn = item.RoleAssignments;
                for (int i = SPRoleAssColn.Count - 1; i >= 0; i--)
                {
                    SPRoleAssColn.Remove(i);
                }

                string nomegrupo = string.Empty;
                for (int i = 0; i < tableGrupos.Rows.Count; i++)
                {
                    nomegrupo = tableGrupos.Rows[i].Field<string>("Empresa") + " " + tableGrupos.Rows[i].Field<string>("Title");
                    SPGroup grupo = web.SiteGroups[nomegrupo];
                    if (grupo != null)
                    {
                        SPRoleType type = Perfil(tableGrupos.Rows[i].Field<string>("Perfil"));

                        SPRoleAssignment roleAssignment = new SPRoleAssignment(grupo);
                        SPRoleDefinition roleDefinition = web.RoleDefinitions.GetByType(type);
                        roleAssignment.RoleDefinitionBindings.Add(roleDefinition);

                        item.RoleAssignments.Add(roleAssignment);
                    }
                }
            }
        }

        private static SPListItemCollection GetListPermissionExclusive(SPWeb web)
        {
            SPList list = web.Webs["configuracoes"].Lists["Permissões Exclusivas"];
            return list.Items;
        }

        private static DataTable ObterGrupos(string nomeEmpresa, SPWeb web)
        {
            SPList list = web.Webs["configuracoes"].Lists.TryGetList("Grupo");

            DataTable table = null;

            SPQuery query = new SPQuery();
            query.Query = "<Where><Eq><FieldRef Name='Empresa' LookupId='FALSE'/><Value Type='Lookup' >" + nomeEmpresa + "</Value></Eq></Where>";
            query.ViewFields = "<FieldRef Name='Title' /><FieldRef Name='Perfil' /><FieldRef Name='Empresa' />";

            SPListItemCollectionPosition collPoss;
            table = list.GetDataTable(query, SPListGetDataTableOptions.None, out collPoss);

            return table;
        }

        private static SPRoleType Perfil(string perfil)
        {
            switch (perfil)
            {
                case "Leitura":
                    return SPRoleType.Reader;
                case "Colaboração":
                    return SPRoleType.Contributor;
                case "Administrador":
                    return SPRoleType.Administrator;
                default:
                    return SPRoleType.None;
            }
        }
    }
}
