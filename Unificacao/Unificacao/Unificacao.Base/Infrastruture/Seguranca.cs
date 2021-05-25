using Furnas.GestaoSPE.Unificacao.Base.Service;
using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furnas.GestaoSPE.Unificacao.Base.Infrastruture
{
    public static class Seguranca
    {

        /// <summary>
        /// Obtem usuário de um grupo
        /// </summary>
        /// <param name="oGroupName">Nome do grupo</param>
        /// <param name="sUserLoginName">Login do usuário</param>
        /// <returns></returns>
        public static SPUser UserInGroup(SPGroup oGroupName, string sUserLoginName)
        {
            SPUser oUser = null;
            try
            {
                oUser = oGroupName.Users[sUserLoginName];
            }
            catch { }
            return oUser;
        }

        /// <summary>
        /// Adiciona um usuário em um grupo.
        /// </summary>
        /// <param name="web">Web que contém o grupo.</param>
        /// <param name="user">Usuário que deverá ser adicionado.</param>
        /// <param name="NMGrupo">Nome do grupo na qual o usuário deverá ser adicionado.</param>
        public static void ConcederPermissao(SPWeb web, SPUser user, string groupName)
        {
            SPGroup grupo = web.SiteGroups[groupName];

            SPUser oUser = Seguranca.UserInGroup(grupo, user.LoginName);
            if (oUser == null)
            {
                web.Groups[groupName].AddUser(user);
            }
        }

        /// <summary>
        /// Remove um usuário de um grupo.
        /// </summary>
        /// <param name="web">Web que contém o grupo.</param>
        /// <param name="user">Usuário que deverá ser removido.</param>
        /// <param name="NMGrupo">Nome do grupo na qual o usuário deverá ser removido.</param>
        public static void RemoverPermissao(SPWeb web, SPUser user, string NMGrupo)
        {
            SPUser oUser = UserInGroup(web.Groups[NMGrupo], user.LoginName);
            if (oUser != null)
            {
                web.Groups[NMGrupo].RemoveUser(oUser);
            }
        }

        /// <summary>
        /// Retorna o SPUser de um campo de lista do tipo Pessoa ou Grupo de valor único.
        /// </summary>
        /// <param name="spListItem">Item da lista que contém o campo Pessoa ou Grupo.</param>
        /// <param name="fieldName">Nome do campo do tipo Pessoa ou Grupo.</param>
        /// <returns>SPUser do usuário contido no campo.</returns>
        public static SPUser ObtemUsuarioDeSPListItem(SPListItem spListItem, String fieldName)
        {
            SPUser spUser = null;

            if (fieldName != string.Empty)
            {
                SPFieldUser field = spListItem.Fields[fieldName] as SPFieldUser;
                if (field != null && spListItem[fieldName] != null)
                {
                    //SPFieldUserValue fieldValue = field.GetFieldValue(spListItem[fieldName].ToString()) as SPFieldUserValue;
                    SPFieldUserValue userValue = new SPFieldUserValue(spListItem.Web, Convert.ToString(spListItem[fieldName]));
                    if (userValue != null)
                    {
                        spUser = userValue.User;
                    }
                }
            }

            return spUser;
        }

        /// <summary>
        /// Criar novos grupos
        /// </summary>
        /// <param name="web"></param>
        /// <param name="roleType">Tipo de acesso que o grupo terá.</param>
        /// <param name="groupName">Nome do grupo</param>
        public static void AdicionarGrupo(SPWeb web, SPRoleType roleType, string groupName)
        {
            SPGroup group = null;

            // Verifica se o grupo existe
            try
            {
                group = web.SiteGroups[groupName];
            }
            catch { }

            //Se não existe cria o grupo
            if (group == null)
            {
                web.SiteGroups.Add(groupName, web.Site.Owner, web.Site.Owner, "Grupo criado para " + groupName);
                group = web.SiteGroups[groupName];

                // Adiciona as permissoes do grupo
                SPRoleDefinition roleDefinition = web.RoleDefinitions.GetByType(roleType);
                SPRoleAssignment roleAssignment = new SPRoleAssignment(group);
                roleAssignment.RoleDefinitionBindings.Add(roleDefinition);
                web.RoleAssignments.Add(roleAssignment);
                web.Update();
            }
        }

        public static void RemoverGrupo(SPWeb web, string nomeGrupo)
        {
            if (GrupoExiste(web.SiteGroups, nomeGrupo))
                web.SiteGroups.Remove(nomeGrupo);
        }

        public static bool GrupoExiste(SPWeb web, string NomeGrupo)
        {
            SPGroup group = null;
            // Verifica se o grupo existe
            try
            {
                group = web.SiteGroups[NomeGrupo];
            }
            catch { }
            if (group != null)
                return true;
            else
                return false;
        }

        public static bool GrupoExiste(SPGroupCollection groups, string name)
        {
            if (string.IsNullOrEmpty(name) ||
                (name.Length > 255) ||
                (groups == null) ||
                (groups.Count == 0))
                return false;
            else
                return (groups.GetCollection(new String[] { name }).Count > 0);
        }

        public static bool GrupoExiste(SPGroupCollection groups, int id)
        {
            if ((id < 0) ||
                (groups == null) ||
                (groups.Count == 0))
                return false;
            else
                return (groups.GetCollection(new Int32[] { id }).Count > 0);
        }

        public static void AddGruposEspecificos(SPWeb web, string groupName)
        {
            web.SiteGroups.Add(groupName, web.Site.Owner, web.Site.Owner, "Grupo criado para " + groupName);
            web.Update();
        }

    }
}
