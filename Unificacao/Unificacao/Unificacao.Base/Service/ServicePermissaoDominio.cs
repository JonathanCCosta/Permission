using Furnas.GestaoSPE.Unificacao.Base.Resources;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furnas.GestaoSPE.Unificacao.Base.Service
{
    public class ServicePermissaoDominio
    {
        public static SPRoleType ObterPerfil(string perfil)
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

        public static SPFieldLookupValueCollection _Dominios { get; set; }
        public static SPWeb _Web { get; set; }
        public static SPWeb _WebConfiguracao { get; set; }
        public static SPFieldLookupValueCollection _SPEs { get; set; }
        public static SPFieldUserValueCollection _Usuarios { get; set; }
        public static SPRoleType _Perfil { get; set; }
        public static SPFieldLookupValueCollection _GruposEspecificos { get; set; }
        public static SPFieldLookupValueCollection _GruposGerente { get; set; }
        //public static SPItemEventProperties _properties { get; set; }

        public ServicePermissaoDominio(SPItemEventProperties properties)
        {
            _Web = properties.Web.ParentWeb;
            _WebConfiguracao = properties.Web;

            if (properties.EventType == SPEventReceiverType.ItemAdded || properties.EventType == SPEventReceiverType.ItemUpdated || properties.EventType == SPEventReceiverType.ItemDeleting)
            {
                //_Perfil = new SPFieldLookupValue(Convert.ToString(properties.ListItem["Perfil"])).LookupValue;
                _SPEs = new SPFieldLookupValueCollection(Convert.ToString(properties.ListItem["ItemSPE"]));
                _Dominios = new SPFieldLookupValueCollection(Convert.ToString(properties.ListItem["Dominio"]));
                _Usuarios = new SPFieldUserValueCollection(_WebConfiguracao, Convert.ToString(properties.ListItem["Usuarios"]));
                _Perfil = ObterPerfil(new SPFieldLookupValue(Convert.ToString(properties.ListItem["Perfil"])).LookupValue);
                _GruposEspecificos = new SPFieldLookupValueCollection(Convert.ToString(properties.ListItem["GruposEspecificos"]));
                _GruposGerente = new SPFieldLookupValueCollection(Convert.ToString(properties.ListItem["PermissaoGerente"]));
            }
            else if (properties.EventType == SPEventReceiverType.ItemAdding)
            {
                string perfil = ObterValorLookupParaItemAdding(Convert.ToInt32(properties.AfterProperties[SPEncode.UrlDecodeAsUrl("Perfil")]), "Perfil", properties.Web);
                _Perfil = ObterPerfil(perfil);
                _Usuarios = new SPFieldUserValueCollection(_WebConfiguracao, Convert.ToString(properties.AfterProperties[SPEncode.UrlDecodeAsUrl("Usuarios")]));
                _SPEs = new SPFieldLookupValueCollection(Convert.ToString(properties.AfterProperties[SPEncode.UrlDecodeAsUrl("ItemSPE")]));
                _Dominios = new SPFieldLookupValueCollection(Convert.ToString(properties.AfterProperties[SPEncode.UrlDecodeAsUrl("Dominio")]));
                _GruposEspecificos = new SPFieldLookupValueCollection(Convert.ToString(properties.AfterProperties[SPEncode.UrlDecodeAsUrl("GruposEspecificos")]));
                _GruposGerente = new SPFieldLookupValueCollection(Convert.ToString(properties.AfterProperties[SPEncode.UrlDecodeAsUrl("PermissaoGerente")]));
            }
            //else if (properties.EventType == SPEventReceiverType.ItemUpdating)
            //{
            //    string perfil = new SPFieldLookupValue(Convert.ToString(properties.AfterProperties[SPEncode.UrlDecodeAsUrl("Perfil")])).LookupValue;
            //    _Perfil = ObterPerfil(perfil);
            //    _SPEs = new SPFieldLookupValueCollection(Convert.ToString(properties.AfterProperties[SPEncode.UrlDecodeAsUrl("ItemSPE")]));
            //    _Dominios = new SPFieldLookupValueCollection(Convert.ToString(properties.AfterProperties[SPEncode.UrlDecodeAsUrl("Dominio")]));
            //    _Usuarios = new SPFieldUserValueCollection(_WebConfiguracao, Convert.ToString(properties.AfterProperties[SPEncode.UrlDecodeAsUrl("Usuarios")]));
            //}
        }

        public ServicePermissaoDominio(SPList lista, SPItemEventProperties properties)
        {
            _Web = lista.ParentWeb;
            _WebConfiguracao = lista.ParentWeb.Webs["configuracoes"];

            DataTable dtDependenciaDominio = ObterDependenciaDominio(lista.Title);

            foreach (DataRow row in dtDependenciaDominio.Rows)
            {
                
            }

            _SPEs = new SPFieldLookupValueCollection(Convert.ToString(properties.ListItem["ItemSPE"]));
            _Dominios = new SPFieldLookupValueCollection(Convert.ToString(properties.ListItem["Dominio"]));
            _Usuarios = new SPFieldUserValueCollection(_WebConfiguracao, Convert.ToString(properties.ListItem["Usuarios"]));
            _Perfil = ObterPerfil(new SPFieldLookupValue(Convert.ToString(properties.ListItem["Perfil"])).LookupValue);
        }

        /// <summary>
        /// Obtem a dependência da lista, ou seja tras a ramificação dos pais do nome da lista passada no parametro.
        /// </summary>
        /// <param name="nomeLista"></param>
        /// <returns></returns>
        private DataTable ObterDependenciaDominio(string nomeLista)
        {
            SPQuery query = new SPQuery();
            query.Query = "<Query><Where><Eq><FieldRef Name='Title' /><Value Type='Text'>"+ nomeLista +"</Value></Eq></Where></Query>";
            query.ViewFields = "<FieldRef Name='Dependencia' />";

            SPListItemCollectionPosition collpos;

            DataTable dt = _WebConfiguracao.Lists["Dominio"].GetDataTable(query, SPListGetDataTableOptions.None, out collpos);

            return dt;
        }

        /// <summary>
        /// Obtem valor lookup. Por default o sharepoint não traz o valor concatenado quando é no item adding. Ex: 1;#value no item adding ele traz 1;# apenas.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="nomeLista"></param>
        /// <param name="web"></param>
        /// <returns></returns>
        private string ObterValorLookupParaItemAdding(int id, string nomeLista, SPWeb web)
        {
            SPList list = web.Lists[nomeLista];
            SPQuery query = new SPQuery();
            query.Query = "<Where><Eq><FieldRef Name='ID' /><Value Type='Counter'>" + id + "</Value></Eq></Where>";
            query.ViewFields = "<FieldRef Name='Title' />";

            SPListItemCollectionPosition collPoss;
            DataTable table = list.GetDataTable(query, SPListGetDataTableOptions.None, out collPoss);

            if (table != null)
                return table.Rows[0].Field<string>("Title");
            else
                return "";
        }

        /// <summary>
        /// Usado para remover premissões quando um item da lista Permissão por Dominio for excluído.
        /// </summary>
        public void RemoverPermissao()
        {
            int[] idsspe = ObterSPEs();
            List<SPListItem> itemsGeral = ItemsDoDominio(idsspe);

            foreach (int speId in idsspe)
            {
                SPListItem itemSPE = _Web.Lists["SPE"].GetItemById(speId);
                int idComplexo = new SPFieldLookupValue(Convert.ToString(itemSPE["Complexo"])).LookupId;
                SPListItem complexo = _Web.Lists["Grupo"].GetItemById(idComplexo);
                itemsGeral.Add(complexo);
            }

            RemoverPermissao(itemsGeral, _Usuarios);
        }

        /// <summary>
        /// Utilizado para remover permissões de items quando for Update.
        /// </summary>
        /// <param name="itemsGeral">Items que terão suas permissões alteradas.</param>
        /// <param name="users">Usuários que terão a permissão removida</param>
        private void RemoverPermissao(List<SPListItem> itemsGeral, SPFieldUserValueCollection users)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                using (SPSite ImpersonatedSite = new SPSite(_Web.Url))
                {
                    using (SPWeb ImpersonatedWeb = ImpersonatedSite.OpenWeb())
                    {
                        List<SPListItem> itemsElevated = ObterItemsComPermissaoElevada(itemsGeral, ImpersonatedWeb);

                        foreach (SPListItem item in itemsElevated)
                        {
                            foreach (SPFieldUserValue user in users)
                            {
                                SPPrincipal principal = user.User as SPPrincipal;
                                item.RoleAssignments.Remove(principal);
                            }
                        }

                        foreach (SPFieldLookupValue grupos in _GruposEspecificos)
                        {
                            SPGroup group = _Web.SiteGroups[grupos.LookupValue];

                            foreach (SPFieldUserValue user in users)
                            {
                                try
                                {
                                    group.RemoveUser(_Web.EnsureUser(user.LookupValue));
                                }
                                catch { }
                            }
                        }

                        foreach (SPFieldLookupValue gruposGerente in _GruposGerente)
                        {
                            SPGroup group = _Web.SiteGroups[gruposGerente.LookupValue];

                            foreach (SPFieldUserValue user in users)
                            {
                                try
                                {
                                    group.RemoveUser(_Web.EnsureUser(user.LookupValue));
                                }
                                catch { }
                            }
                        }
                    }
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item">Item da lista Permissão por Dominio</param>
        public void AtualizarPermissoes(SPItemEventProperties properties)
        {
            int count = properties.ListItem.Versions.Count;
            if (count > 1)
            {
                List<SPListItem> itemsOld, itemsNew, itemsRestante;
                SPListItemVersion itemVersion = properties.ListItem.Versions[1];
                if (itemVersion != null)
                {
                    int[] idsspe = ObterSPEs();
                    _Dominios = new SPFieldLookupValueCollection(Convert.ToString(itemVersion["Dominio"]));

                    itemsOld = ItemsDoDominio(idsspe);

                    foreach (int speId in idsspe)
                    {
                        SPListItem itemSPE = _Web.Lists["SPE"].GetItemById(speId);
                        int idComplexo = new SPFieldLookupValue(Convert.ToString(itemSPE["Complexo"])).LookupId;
                        SPListItem complexo = _Web.Lists["Grupo"].GetItemById(idComplexo);
                        itemsOld.Add(complexo);
                    }
                    SPFieldLookupValueCollection _collGruposEspecificosOld = new SPFieldLookupValueCollection(Convert.ToString(itemVersion["GruposEspecificos"]));
                    SPFieldLookupValueCollection _collGruposEspecificosNew = _GruposEspecificos;

                    SPFieldLookupValueCollection _collGruposGerenteOld = new SPFieldLookupValueCollection(Convert.ToString(itemVersion["PermissaoGerente"]));
                    SPFieldLookupValueCollection _collGruposGerenteNew = _GruposGerente;
                    //Obter usuários que terão a permissão removida.
                    SPFieldUserValueCollection collUsersOld = new SPFieldUserValueCollection(_WebConfiguracao, Convert.ToString(itemVersion["Usuarios"]));
                    //Remove as permissões dos usuários da desta versão.
                    _GruposEspecificos = _collGruposEspecificosOld;
                    _GruposGerente = _collGruposGerenteOld;
                    RemoverPermissao(itemsOld, collUsersOld);
                    
                    _Dominios = new SPFieldLookupValueCollection(Convert.ToString(properties.ListItem["Dominio"]));
                    itemsNew = ItemsDoDominio(idsspe);

                    _GruposEspecificos = _collGruposEspecificosNew;
                    _GruposGerente = _collGruposGerenteNew;
                    //Adiciona as permissões para os usuários desta versão.
                    AtribuirPermissao();

                    //Não vai usar este opção abaixo.
                    //itemsRestante = itemsGeralVersion.Except<SPListItem>(itemsGeral).ToList();
                }
            }
        }

        public void AtribuirPermissao()
        {
            //IDs veem da pagina. Item selecionado pelo cliente.
            int[] idsspe = ObterSPEs();

            foreach (int idSPE in idsspe)
            {
                List<SPListItem> itemsGeral = ItemsDoDominio(idSPE);

                //Obtenho o complexo
                //foreach (int speId in idsspe)
                //{
                    SPListItem itemSPE = _Web.Lists["SPE"].GetItemById(idSPE);
                    int idComplexo = new SPFieldLookupValue(Convert.ToString(itemSPE["Complexo"])).LookupId;
                    SPListItem complexo = _Web.Lists["Grupo"].GetItemById(idComplexo);
                    itemsGeral.Add(complexo);
                //}

                //Construir método que retornará os domínios em arvore de forma que você possa buscar os itens a serem permissionados
                //string[] estruturadominio = { "SPE", "Empreendimento", "Obra", "Quantificacao" };

                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    using (SPSite ImpersonatedSite = new SPSite(_Web.Url))
                    {
                        using (SPWeb ImpersonatedWeb = ImpersonatedSite.OpenWeb())
                        {
                            List<SPListItem> itemsElevated = ObterItemsComPermissaoElevada(itemsGeral, ImpersonatedWeb);

                            int verificaDominio = 0;

                            foreach (SPListItem item in itemsElevated)
                            {
                                verificaDominio = 0;
                                
                                foreach (SPFieldLookupValue dominio in _Dominios)
                                {
                                    if (dominio.LookupValue == item.ParentList.Title)
                                    {
                                        verificaDominio = 1;
                                    }
                                }

                                if (verificaDominio == 1)
                                {
                                    AdicionarPermissaoDominioNoItem(item, _Perfil);
                                }
                                else
                                {
                                    AdicionarPermissaoDominioNoItem(item, SPRoleType.Reader);
                                }
                            }
                        }
                    }
                });
            }

            AdicionaUsuarioGruposEspecificos();
            AdicionaUsuarioGruposGerente();
        }

        /// <summary>
        /// Efetiva a permissão no item que sofrerá a permissão.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="type"></param>
        private void AdicionarPermissaoDominioNoItem(SPListItem item, SPRoleType role)
        {
            //Quebra a herança
            if (!item.HasUniqueRoleAssignments)
            {
                item.BreakRoleInheritance(false, true);
            }

            foreach (SPFieldUserValue user in _Usuarios)
            {
                SPUser userTemp;
                if (user.LookupId < 0)
                    userTemp = _Web.EnsureUser(user.LookupValue);
                else
                    userTemp = user.User;

                SPRoleAssignment roleAssignment = new SPRoleAssignment(userTemp);
                //SPRoleDefinition roleDefinition = _Web.RoleDefinitions.GetByType(_Perfil);
                SPRoleDefinition roleDefinition = _Web.RoleDefinitions.GetByType(role);
                roleAssignment.RoleDefinitionBindings.Add(roleDefinition);

                item.RoleAssignments.Add(roleAssignment);
            }
        }

        private List<SPListItem> ObterItemsComPermissaoElevada(List<SPListItem> itemsGeral, SPWeb web)
        {
            List<SPListItem> collTemp = new List<SPListItem>();
            SPList GenericList;
            SPListItem GenericItem;

            foreach (SPListItem item in itemsGeral)
            {
                GenericList = web.Lists[item.ParentList.ID];
                GenericItem = GenericList.GetItemById(item.ID);
                collTemp.Add(GenericItem);
            }
            return collTemp;
        }

        private void AdicionaUsuarioGruposEspecificos()
        {
            SPSecurity.RunWithElevatedPrivileges(delegate()
               {
                   using (SPSite ImpersonatedSite = new SPSite(_Web.Url))
                   {
                       using (SPWeb ImpersonatedWeb = ImpersonatedSite.OpenWeb())
                       {
                           foreach (SPFieldLookupValue grupos in _GruposEspecificos)
                           {
                               SPGroup group = _Web.SiteGroups[grupos.LookupValue];

                               foreach (SPFieldUserValue user in _Usuarios)
                               {
                                   group.AddUser(_Web.EnsureUser(user.LookupValue));
                               }
                           }
                       }
                   }
               });
        }

        private void AdicionaUsuarioGruposGerente()
        {
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                using (SPSite ImpersonatedSite = new SPSite(_Web.Url))
                {
                    using (SPWeb ImpersonatedWeb = ImpersonatedSite.OpenWeb())
                    {
                        foreach (SPFieldLookupValue grupos in _GruposGerente)
                        {
                            SPGroup group = _Web.SiteGroups[grupos.LookupValue];

                            foreach (SPFieldUserValue user in _Usuarios)
                            {
                                group.AddUser(_Web.EnsureUser(user.LookupValue));
                            }
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Obtem os items que terão permissões para usuários do domínio.
        /// </summary>
        /// <param name="idsspe"></param>
        /// <returns>Lista com todos os items</returns>
        private List<SPListItem> ItemsDoDominio(int[] idsspe)
        {
            SPQuery query;
            SPQuery query2;
            List<SPListItem> itemsGeralTemp = new List<SPListItem>();
            foreach (int idSPE in idsspe)
            {
                int[] ids = { idSPE };
                Dictionary<string, string[]> collDominios = ObterDominios("Dependencia");
                Dictionary<string, string[]> collDominios2 = ObterDominios("Dependencia2");

                Dictionary<string, string[]> collLookupDepencias = ObterLookupDepencias("Lookup_x0020_Dependencia");
                Dictionary<string, string[]> collLookupDepencias2 = ObterLookupDepencias("Lookup_x0020_Dependencia2");

                for (int i = 0; i < collDominios.Count; i++)
                {
                    string dominio = collDominios.Keys.ElementAt(i);
                    string dependencia = collLookupDepencias.Keys.ElementAt(i);
                    query = MontarQueryIDS(ids, null);
                    ItemsPorDominio(ref query, ref itemsGeralTemp, collDominios[dominio], collLookupDepencias[dependencia]);
                }

                if (collLookupDepencias2.Count > 0 && collDominios2.Count > 0)
                {
                    for (int i = 0; i < collDominios2.Count; i++)
                    {
                        string dominio = collDominios2.Keys.ElementAt(i);
                        string dependencia = collLookupDepencias2.Keys.ElementAt(i);
                        query2 = MontarQueryIDS(ids, null);
                        ItemsPorDominio(ref query2, ref itemsGeralTemp, collDominios2[dominio], collLookupDepencias2[dependencia]);
                    }
                }

                /*foreach (KeyValuePair<string, string[]> dominio in collDominios)
                {
                    query = MontarQueryIDS(ids, null);
                    ItemsPorDominio(ref query, ref itemsGeralTemp, dominio.Value);
                }*/
            }

            List<SPListItem> itemsGeral = itemsGeralTemp.GroupBy(g => new { g.ID, g.ParentList.Title }).Select(p => p.First()).ToList();
            return itemsGeral;
        }

        private List<SPListItem> ItemsDoDominio(int idsspe)
        {
            SPQuery query;
            SPQuery query2;
            List<SPListItem> itemsGeralTemp = new List<SPListItem>();
            //foreach (int idSPE in idsspe)
            //{
            //int[] ids = { idSPE };
            int[] ids = { idsspe };
            Dictionary<string, string[]> collDominios = ObterDominios("Dependencia");
            Dictionary<string, string[]> collDominios2 = ObterDominios("Dependencia2");

            Dictionary<string, string[]> collLookupDepencias = ObterLookupDepencias("Lookup_x0020_Dependencia");
            Dictionary<string, string[]> collLookupDepencias2 = ObterLookupDepencias("Lookup_x0020_Dependencia2");

            for (int i = 0; i < collDominios.Count; i++)
            {
                string dominio = collDominios.Keys.ElementAt(i);
                string dependencia = collLookupDepencias.Keys.ElementAt(i);
                query = MontarQueryIDS(ids, null);
                ItemsPorDominio(ref query, ref itemsGeralTemp, collDominios[dominio], collLookupDepencias[dependencia]);
            }

            if (collLookupDepencias2.Count > 0 && collDominios2.Count > 0)
            {
                for (int i = 0; i < collDominios2.Count; i++)
                {
                    string dominio = collDominios2.Keys.ElementAt(i);
                    string dependencia = collLookupDepencias2.Keys.ElementAt(i);
                    query2 = MontarQueryIDS(ids, null);
                    ItemsPorDominio(ref query2, ref itemsGeralTemp, collDominios2[dominio], collLookupDepencias2[dependencia]);
                }
            }

            /*foreach (KeyValuePair<string, string[]> dominio in collDominios)
            {
                query = MontarQueryIDS(ids, null);
                ItemsPorDominio(ref query, ref itemsGeralTemp, dominio.Value);
            }*/
            //}

            List<SPListItem> itemsGeral = itemsGeralTemp.GroupBy(g => new { g.ID, g.ParentList.Title }).Select(p => p.First()).ToList();
            return itemsGeral;
        }

        private void ItemsPorDominio(ref SPQuery query, ref List<SPListItem> itemsGeral, string[] dominio, string[] dependencia)
        {
            int count = dominio.Count();

            for (int i = count - 1; i >= 0; i--)
            {
                List<SPListItem> itemsFilhos = new List<SPListItem>();
                itemsFilhos.AddRange(BuscaFilho(query, _Web.Lists[dominio[i]]));
                if (itemsFilhos.Count < 1)
                    break;//Uma vez que não retorna um item não existe mais ramos abaixo deste ultimo verificado por isso interrompemos o processo para este dominio aqui.
                itemsGeral.AddRange(itemsFilhos);
                //query = MontarQueryIDS(RetornaIDSFilho(itemsFilhos), itemsFilhos[0].ParentList.Title);
                query = MontarQueryIDS(RetornaIDSFilho(itemsFilhos), dependencia[i]);
            }
        }

        private int[] RetornaIDSFilho(List<SPListItem> itemsFilhos)
        {
            int count = itemsFilhos.Count;
            int[] idsFilhos = new int[count];
            for (int i = 0; i < count; i++)
            {
                idsFilhos[i] = itemsFilhos[i].ID;
            }
            return idsFilhos;
        }

        private List<SPListItem> BuscaFilho(SPQuery query, SPList list)
        {
            List<SPListItem> collSPes = list.GetItems(query).OfType<SPListItem>().ToList();
            return collSPes;
        }

        private SPQuery MontarQueryIDS(int[] ids, string lookupCollum)
        {
            List<string> objColumns = new List<string>();
            bool islookup = string.IsNullOrEmpty(lookupCollum);

            foreach (int id in ids)
            {
                if (!islookup)
                    objColumns.Add(lookupCollum + ";Lookup;Eq;" + id);
                else
                    objColumns.Add("ID;Counter;Eq;" + id);

            }

            SPQuery query = new SPQuery();
            query.Query = Util.CreateCAMLQuery(objColumns, "Or", true, !islookup);

            return query;
        }

        private Dictionary<string, string[]> ObterDominios(string collumn_dominio)
        {
            List<string> objColumns = new List<string>();

            foreach (SPFieldLookupValue dominio in _Dominios)
            {
                objColumns.Add("ID;Counter;Eq;" + dominio.LookupId);
            }
            SPQuery query = new SPQuery();
            query.Query = Util.CreateCAMLQuery(objColumns, "Or", true, false);
            query.ViewFields = "<FieldRef Name='Title' /><FieldRef Name='Nome' /><FieldRef Name='" + collumn_dominio +"' />";

            SPList list = _WebConfiguracao.Lists["Dominio"];

            Dictionary<string, string[]> Dominios;

            Dominios = ObterItemsDoDominio(query, list, collumn_dominio);

            return Dominios;
        }

        private Dictionary<string, string[]> ObterLookupDepencias(string collumn_dependencia)
        {
            List<string> objColumns = new List<string>();

            foreach (SPFieldLookupValue dominio in _Dominios)
            {
                objColumns.Add("ID;Counter;Eq;" + dominio.LookupId);
            }

            SPQuery query = new SPQuery();
            query.Query = Util.CreateCAMLQuery(objColumns, "Or", true, false);
            query.ViewFields = "<FieldRef Name='Nome' /><FieldRef Name='" + collumn_dependencia + "' />";

            SPList list = _WebConfiguracao.Lists["Dominio"];

            Dictionary<string, string[]> Dependencias;

            Dependencias = ObterItemsDoLookupDependencia(query, list, collumn_dependencia);

            return Dependencias;
        }

        private Dictionary<string, string[]> ObterItemsDoDominio(SPQuery query, SPList list, string collumn_dominio)
        {
            SPListItemCollectionPosition collPoss;
            DataTable table = list.GetDataTable(query, SPListGetDataTableOptions.None, out collPoss);

            Dictionary<string, string[]> Dominios = new Dictionary<string, string[]>();

            if (collumn_dominio == "Dependencia2")
            {
                foreach (DataRow row in table.Rows)
                {
                    if (row.Field<string>(collumn_dominio) != null)
                    {
                        Dominios.Add(row.Field<string>("Nome"), (row.Field<string>("Nome") + ";" + row.Field<string>(collumn_dominio)).Split(';'));
                    }
                }
            }
            else
            {
                foreach (DataRow row in table.Rows)
                {
                    Dominios.Add(row.Field<string>("Nome"), (row.Field<string>("Nome") + ";" + row.Field<string>(collumn_dominio)).Split(';'));
                }
            }


            return Dominios;
        }

        private Dictionary<string, string[]> ObterItemsDoLookupDependencia(SPQuery query, SPList list, string collumn_dependencia)
        {
            SPListItemCollectionPosition collPoss;
            DataTable table = list.GetDataTable(query, SPListGetDataTableOptions.None, out collPoss);

            Dictionary<string, string[]> Dependencias = new Dictionary<string, string[]>();

            if (collumn_dependencia == "Lookup_x0020_Dependencia2")
            {
                foreach (DataRow row in table.Rows)
                {
                    if (row.Field<string>(collumn_dependencia) != null)
                    {
                        Dependencias.Add(row.Field<string>("Nome"), (row.Field<string>("Nome") + ";" + row.Field<string>(collumn_dependencia)).Split(';'));
                    }
                }
            }
            else
            {
                foreach (DataRow row in table.Rows)
                {
                    Dependencias.Add(row.Field<string>("Nome"), (row.Field<string>("Nome") + ";" + row.Field<string>(collumn_dependencia)).Split(';'));
                }
            }

            return Dependencias;
        }

        private int[] ObterSPEs()
        {
            List<string> objColumns = new List<string>();

            foreach (SPFieldLookupValue spe in _SPEs)
            {
                objColumns.Add("ID;Counter;Eq;" + spe.LookupId);
            }

            SPQuery query = new SPQuery();
            query.Query = Util.CreateCAMLQuery(objColumns, "Or", true, false);
            query.ViewFields = "<FieldRef Name='ID' />";
            SPList list = _Web.Lists["SPE"];

            SPListItemCollectionPosition collPoss;
            DataTable table = list.GetDataTable(query, SPListGetDataTableOptions.None, out collPoss);

            int count = table.Rows.Count;

            int[] ids = new int[count];

            for (int i = 0; i < count; i++)
            {
                ids[i] = table.Rows[i].Field<int>("ID");
            }

            return ids;
        }






        #region Remover depois
        //public void Atribuirpermissao()
        //{
        //    DataTable dtDominios = ObterDominios();
        //    Dictionary<string, string[]> Dominios = new Dictionary<string, string[]>();

        //    foreach (DataRow row in dtDominios.Rows)
        //    {
        //        Dominios.Add(row.Field<string>("Nome"), row.Field<string>("Dependencia").Split(';'));
        //    }

        //    SPListItemCollection collSPEs = ObterSPEs();

        //    foreach (SPListItem item in collSPEs)
        //    {
        //        List<SPListItem> items = new List<SPListItem>();
        //        items.Add(item);

        //        Dictionary<int, List<SPListItem>> dic = new Dictionary<int, List<SPListItem>>();
        //        dic.Add(1, items);

        //        foreach (KeyValuePair<string, string[]> dominio in Dominios)
        //        {
        //            int count = dominio.Value.Count(), aux = 0;

        //            for (int i = count - 1; i >= 0; i--)
        //            {
        //                int countAnt = items.Count, countAfter = 0;
        //                foreach (KeyValuePair<int, List<SPListItem>> its in dic)
        //                {
        //                    foreach (SPListItem it in its.Value)
        //                    {
        //                        //List<SPListItem> itemsDependencia = ObterItemsDependencia(items[aux].ID, dominio, i);
        //                        List<SPListItem> itemsDependencia = ObterItemsDependencia(it.ID, dominio, i);

        //                        //items.AddRange(itemsDependencia);
        //                        aux = aux + 1;
        //                        dic.Add(itemsDependencia.Count, itemsDependencia); 
        //                    }
        //                }
        //            }
        //        }

        //        foreach (SPListItem oItem in items)
        //        {
        //            DefinirPermissao(oItem);
        //        }
        //    }

        //}

        //private void DefinirPermissao(SPListItem item)
        //{
        //    throw new NotImplementedException();
        //}

        //private List<SPListItem> ObterItemsDependencia(int id, KeyValuePair<string, string[]> dominio, int i)
        //{
        //    List<SPListItem> items = _Web.Lists[dominio.Value[i - 1]].Items.OfType<SPListItem>().Where(p =>
        //       new SPFieldLookupValue(Convert.ToString(p[dominio.Value[i]])).LookupId == id).ToList();

        //    return items;
        //}

        ////Metodo para obter dominios DataTable
        //private DataTable ObterDominios()
        //{
        //    List<string> objColumns = new List<string>();

        //    foreach (SPFieldLookupValue dominio in _Dominios)
        //    {
        //        objColumns.Add("ID;Counter;Eq;" + dominio.LookupId);
        //    }

        //    SPQuery query = new SPQuery();
        //    query.Query = Util.CreateCAMLQuery(objColumns, "Or", true);
        //    query.ViewFields = "<FieldRef Name='Title' /><FieldRef Name='Nome' /><FieldRef Name='Dependencia' />";

        //    SPList list = _WebConfiguracao.Lists["Dominio"];

        //    SPListItemCollectionPosition collPoss;
        //    DataTable table = list.GetDataTable(query, SPListGetDataTableOptions.None, out collPoss);

        //    return table;
        //}

        //private SPListItemCollection ObterSPEs()
        //{
        //    List<string> objColumns = new List<string>();

        //    foreach (SPFieldLookupValue spe in _SPEs)
        //    {
        //        objColumns.Add("ID;Counter;Eq;" + spe.LookupId);
        //    }

        //    SPQuery query = new SPQuery();
        //    query.Query = Util.CreateCAMLQuery(objColumns, "Or", true);

        //    SPList list = _Web.Lists["SPE"];

        //    SPListItemCollection collSPes = list.GetItems(query);

        //    return collSPes;
        //}
        #endregion
    }
}
