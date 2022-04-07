﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using NewLife.Web;
using XCode.Membership;
using Microsoft.AspNetCore.Mvc.Filters;

namespace NewLife.Cube.Admin.Controllers
{
    /// <summary>角色控制器</summary>
    [DisplayName("角色")]
    [Description("系统基于角色授权，每个角色对不同的功能模块具备添删改查以及自定义权限等多种权限设定。")]
    [Area("Admin")]
    [Menu(90, true, Icon = "fa-user-plus")]
    public class RoleController : EntityController<Role>
    {
        static RoleController()
        {
            ListFields.RemoveField("ID", "Ex1", "Ex2", "Ex3", "Ex4", "Ex5", "Ex6", "UpdateUserID", "UpdateIP", "Remark");
            ListFields.RemoveCreateField();

            {
                var df = ListFields.AddListField("Remark", "UpdateUser");
            }
        }

        /// <summary>动作执行前</summary>
        /// <param name="filterContext"></param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            ViewBag.HeaderTitle = "角色管理";
            //ViewBag.HeaderContent = "系统基于角色授权，每个角色对不同的功能模块具备添删改查以及自定义权限等多种权限设定。";

            var bs = this.Bootstrap();
            bs.MaxColumn = 1;

            base.OnActionExecuting(filterContext);
        }

        /// <summary>搜索数据集</summary>
        /// <param name="p"></param>
        /// <returns></returns>
        protected override IEnumerable<Role> Search(Pager p)
        {
            var id = p["id"].ToInt(-1);
            if (id > 0)
            {
                var list = new List<Role>();
                var entity = Role.FindByID(id);
                if (entity != null) list.Add(entity);
                return list;
            }

            return Role.Search(p["dtStart"].ToDateTime(), p["dtEnd"].ToDateTime(), p["Q"], p);
        }

        /// <summary>保存</summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public override ActionResult Edit(Role entity)
        {
            // 保存权限项
            var menus = XCode.Membership.Menu.Root.AllChilds;
            //var pfs = EnumHelper.GetDescriptions<PermissionFlags>().Where(e => e.Key > PermissionFlags.None);
            var dels = new List<Int32>();
            // 遍历所有权限资源
            foreach (var item in menus)
            {
                // 是否授权该项
                var has = GetBool("p" + item.ID);
                if (!has)
                    dels.Add(item.ID);
                else
                {
                    // 遍历所有权限子项
                    var any = false;
                    foreach (var pf in item.Permissions)
                    {
                        var has2 = GetBool("pf" + item.ID + "_" + pf.Key);

                        if (has2)
                            entity.Set(item.ID, (PermissionFlags)pf.Key);
                        else
                            entity.Reset(item.ID, (PermissionFlags)pf.Key);
                        any |= has2;
                    }
                    // 如果原来没有权限，这是首次授权，且右边没有勾选任何子项，则授权全部
                    if (!any & !entity.Has(item.ID)) entity.Set(item.ID);
                }
            }
            // 删除已经被放弃权限的项
            foreach (var item in dels)
            {
                if (entity.Has(item)) entity.Permissions.Remove(item);
            }

            return base.Edit(entity);
        }

        /// <summary>验证实体对象</summary>
        /// <param name="entity"></param>
        /// <param name="type"></param>
        /// <param name="post"></param>
        /// <returns></returns>
        protected override Boolean Valid(Role entity, DataObjectMethodType type, Boolean post)
        {
            var rs = base.Valid(entity, type, post);

            // 清空缓存
            if (post) Role.Meta.Session.ClearCache($"{type}-{entity}", true);

            return rs;
        }

        private Boolean GetBool(String name)
        {
            var v = GetRequest(name);
            if (v.IsNullOrEmpty()) return false;

            v = v.Split(",")[0];

            if (!v.EqualIgnoreCase("true", "false")) throw new XException("非法布尔值Request[{0}]={1}", name, v);

            return v.ToBoolean();
        }
    }
}