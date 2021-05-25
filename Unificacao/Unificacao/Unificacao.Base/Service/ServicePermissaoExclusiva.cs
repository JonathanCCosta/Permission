using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furnas.GestaoSPE.Unificacao.Base.Service
{
    public class ServicePermissaoExclusiva
    {
        public SPUser _User { get; set; }
        public SPWeb _Web { get; set; }
        public SPWeb _WebConfiguracao { get; set; }
        public int _IdItem { get; set; }
        public string _NomeLista { get; set; }

        public ServicePermissaoExclusiva()
        {

        }

        public ServicePermissaoExclusiva(SPItemEventProperties properties, SPUser user)
        {
            _User = user;
            _Web = properties.Web;
            _IdItem = properties.ListItemId;
            _NomeLista = properties.ListTitle;
            if (_Web.Name != "configuracoes")
            {
                _WebConfiguracao = properties.Web.Webs["configuracoes"];
            }
            else
            {
                _WebConfiguracao = properties.Web;
            }
        }

        public ServicePermissaoExclusiva(SPWeb web, string nomeLista)
        {
            _Web = web;
            _NomeLista = nomeLista;
            if (_Web.Name != "configuracoes")
            {
                _WebConfiguracao = web.Webs["configuracoes"];
            }
            else
            {
                _WebConfiguracao = web;
            }
        }

        public void GarantirPermissaoEmMassa(string nomeEmpresa)
        {
            DataTable tableGrupos = ObterGrupos(nomeEmpresa);

            if (tableGrupos != null)
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    using (SPSite ImpersonatedSite = new SPSite(_Web.Url))
                    {
                        using (SPWeb ImpersonatedWeb = ImpersonatedSite.OpenWeb())
                        {
                            SPListItemCollection collItens = ImpersonatedWeb.Lists[_NomeLista].Items;
                            foreach (SPListItem item in collItens)
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
                                    SPGroup grupo = ImpersonatedWeb.SiteGroups[nomegrupo];
                                    if (grupo != null)
                                    {
                                        SPRoleType type = Perfil(tableGrupos.Rows[i].Field<string>("Perfil"));

                                        SPRoleAssignment roleAssignment = new SPRoleAssignment(grupo);
                                        SPRoleDefinition roleDefinition = ImpersonatedWeb.RoleDefinitions.GetByType(type);
                                        roleAssignment.RoleDefinitionBindings.Add(roleDefinition);

                                        item.RoleAssignments.Add(roleAssignment);
                                    }
                                }

                            }
                        }
                    }
                });
            }

        }

        public void UsuarioPodeCriarItem()
        {
            if (TemPermissaoExclusiva(_NomeLista))
            {
                DataTable tableGrupos = ObterGrupos();
                if (tableGrupos == null)
                    throw new Exception("Você não pode criar itens nesta lista. Você precisa estar cadastrado em uma empresa primeiro.");
            }
        }

        public void GarantirPermissao()
        {
            if (TemPermissaoExclusiva(_NomeLista))
            {
                DataTable tableGrupos = ObterGrupos();

                if (tableGrupos != null)
                {
                    SPSecurity.RunWithElevatedPrivileges(delegate()
                    {
                        using (SPSite ImpersonatedSite = new SPSite(_Web.Url))
                        {
                            using (SPWeb ImpersonatedWeb = ImpersonatedSite.OpenWeb())
                            {
                                SPListItem item = ImpersonatedWeb.Lists[_NomeLista].GetItemById(_IdItem);

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
                                    if (_Web.Name != "configuracoes")
                                    {
                                        SPGroup grupo = ImpersonatedWeb.SiteGroups[nomegrupo];
                                        if (grupo != null)
                                        {
                                            SPRoleType type = Perfil(tableGrupos.Rows[i].Field<string>("Perfil"));

                                            SPRoleAssignment roleAssignment = new SPRoleAssignment(grupo);
                                            SPRoleDefinition roleDefinition = ImpersonatedWeb.RoleDefinitions.GetByType(type);
                                            roleAssignment.RoleDefinitionBindings.Add(roleDefinition);

                                            item.RoleAssignments.Add(roleAssignment);
                                        }
                                    }
                                    else
                                    {
                                        if (nomegrupo.Contains("Administração"))
                                        {
                                            SPGroup grupo = ImpersonatedWeb.SiteGroups[nomegrupo];
                                            if (grupo != null)
                                            {
                                                SPRoleType type = Perfil(tableGrupos.Rows[i].Field<string>("Perfil"));

                                                SPRoleAssignment roleAssignment = new SPRoleAssignment(grupo);
                                                SPRoleDefinition roleDefinition = ImpersonatedWeb.RoleDefinitions.GetByType(type);
                                                roleAssignment.RoleDefinitionBindings.Add(roleDefinition);

                                                item.RoleAssignments.Add(roleAssignment);
                                            }
                                        }
                                    }

                                if (_Web.Name != "configuracoes")
                                {
                                    SPList lista = _WebConfiguracao.Lists["Permissão por Domínio"]; //ImpersonatedWeb.Lists["Permissão por Domínio"];

                                    string NameList = item.ParentList.Title;
                                    List<SPListItem> collItens = lista.Items.OfType<SPListItem>().Where(p => new SPFieldMultiChoiceValue(Convert.ToString(p["Dominio"])).ToString().Contains(NameList)).ToList();

                                    foreach (SPListItem itemDominio in collItens)
                                    {
                                        string permissao = new SPFieldLookupValue(Convert.ToString(itemDominio["Perfil"])).LookupValue;
                                        SPFieldUserValueCollection grupo_user = new SPFieldUserValueCollection(_WebConfiguracao, Convert.ToString(itemDominio["Usuarios"]));
                                        foreach (SPFieldUserValue user in grupo_user)
                                        {
                                            SPRoleAssignment roleAssingDominio = new SPRoleAssignment(user.User);
                                            SPRoleDefinition roleDefDominio = ImpersonatedWeb.RoleDefinitions[permissao];
                                            roleAssingDominio.RoleDefinitionBindings.Add(roleDefDominio);

                                            item.RoleAssignments.Add(roleAssingDominio);
                                        }
                                    }
                                }

                            }
                        }
                    });
                }
            }
        }

        /// <summary>
        /// Garante que todo usuário convidado (Que foi adicionado na lista "Permissão pod Dominio") tenha acesso a este item.
        /// </summary>
        public void PermissaoUsuarioDominio(ref SPListItem item)
        {

        }

        private DataTable ObterGrupos(string nomeEmpresa)
        {
            SPList list = _WebConfiguracao.Lists.TryGetList("Grupo");

            DataTable table = null;

            SPQuery query = new SPQuery();
            query.Query = "<Where><Eq><FieldRef Name='Empresa' LookupId='FALSE'/><Value Type='Lookup' >" + nomeEmpresa + "</Value></Eq></Where>";
            query.ViewFields = "<FieldRef Name='Title' /><FieldRef Name='Perfil' /><FieldRef Name='Empresa' />";

            SPListItemCollectionPosition collPoss;
            table = list.GetDataTable(query, SPListGetDataTableOptions.None, out collPoss);

            return table;
        }

        protected DataTable ObterGrupos()
        {
            SPList list = _WebConfiguracao.Lists.TryGetList("Grupo");

            DataTable tableGruposDoUsuario = ObterGrupoUsuario(list);
            DataTable table = null;
            //Estava com || coloquei && testar pra ver não deu erro
            if (tableGruposDoUsuario != null && tableGruposDoUsuario.Rows.Count > 0)
            {
                SPQuery query = new SPQuery();
                query.Query = "<Where><Eq><FieldRef Name='Empresa' LookupId='FALSE'/><Value Type='Lookup' >" + tableGruposDoUsuario.Rows[0].Field<string>("Empresa") + "</Value></Eq></Where>";
                query.ViewFields = "<FieldRef Name='Title' /><FieldRef Name='Perfil' /><FieldRef Name='Empresa' />";

                SPListItemCollectionPosition collPoss;
                table = list.GetDataTable(query, SPListGetDataTableOptions.None, out collPoss);
            }
            return table;
        }

        protected DataTable ObterGrupoUsuario(SPList list)
        {
            DataTable table = null;

            if (list != null)
            {
                //Verifica se a lista do item alterado pertence a "Listas da SPE". Se pertencer da iníco ao processo de permissionamento do item.
                SPQuery query = new SPQuery();
                query.Query = "<Where><Eq><FieldRef Name='Usuarios' LookupId='TRUE'/><Value Type='Int' >" + _User.ID + "</Value></Eq></Where>";
                query.ViewFields = "<FieldRef Name='Title' /><FieldRef Name='Perfil' /><FieldRef Name='Empresa' />";

                SPListItemCollectionPosition collPoss;
                table = list.GetDataTable(query, SPListGetDataTableOptions.None, out collPoss);
            }
            return table;
        }

        /// <summary>
        /// Verifica se o item que esta sendo criado deve ter permissões exclusivas.
        /// </summary>
        /// <param name="titulo">Título da lista do item</param>
        /// <param name="refWeb"></param>
        /// <returns>Se verdadeiro, então o item pode ter suas permissões alteradas.</returns>
        protected bool TemPermissaoExclusiva(string titulo)
        {
            SPList list = _WebConfiguracao.Lists.TryGetList("Permissões Exclusivas");
            bool isValid = false;

            if (list != null)
            {
                //Verifica se a lista do item alterado pertence a "Listas da SPE". Se pertencer da iníco ao processo de permissionamento do item.
                SPQuery query = new SPQuery();
                query.Query = "<Where><Eq><FieldRef Name='Title' /><Value Type='Text'>" + titulo + "</Value></Eq></Where>";

                SPListItemCollectionPosition collPoss;
                DataTable table = list.GetDataTable(query, SPListGetDataTableOptions.None, out collPoss);

                if (table != null && table.Rows.Count > 0)
                    isValid = true;
            }

            return isValid;
        }

        public SPRoleType Perfil(string perfil)
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
