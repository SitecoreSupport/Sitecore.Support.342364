using Sitecore.Publishing;
using Sitecore.Data.Items;
using System;
using System.Collections.Generic;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Events;

namespace Sitecore.Support.Commerce.Engine.Connect.Events
{
    public class CommercePublishCacheRefresh
    {
        public virtual void ClearCache(object sender, EventArgs args)
        {
            var publishOptions = this.GetPublishOptions(args);
            if (publishOptions?.TargetDatabase != null)
            {
                Sitecore.Commerce.Engine.Connect.EngineConnectUtility.RemoveItemFromSitecoreCaches(Sitecore.Commerce.Engine.Connect.CommerceConstants.KnownItemIds.CatalogsItem, publishOptions.TargetDatabaseName);

                var contentItem = publishOptions.TargetDatabase.GetItem(@"/sitecore/content");

                var list = new List<Item>();
                if (contentItem != null)
                {
                    this.CheckForNavChildren(publishOptions.RootItem, list);

                    foreach (var item in list)
                    {
                        Sitecore.Commerce.Engine.Connect.EngineConnectUtility.RemoveItemFromSitecoreCaches(item.ID, publishOptions.TargetDatabaseName);
                    }
                }
            }
        }

        private void CheckForNavChildren(Item item, ICollection<Item> list)
        {
            if (item == null)
            {
                return;
            }

            if (item.TemplateID == Sitecore.Commerce.Engine.Connect.CommerceConstants.KnownTemplateIds.CommerceNavigationItemTemplate)
            {
                list.Add(item);

                return;
            }

            foreach (Item child in item.GetChildren())
            {
                this.CheckForNavChildren(child, list);
            }
        }

        private ItemPublishOptions GetPublishOptions(EventArgs args)
        {
            var sitecoreArgs = args as Sitecore.Events.SitecoreEventArgs;
            if (sitecoreArgs != null)
            {
                foreach (var parameter in sitecoreArgs.Parameters)
                {
                    var publisher = parameter as Publisher;
                    if (publisher != null)
                    {
                        return new ItemPublishOptions
                        {
                            TargetDatabase = publisher.Options.TargetDatabase,
                            TargetDatabaseName = publisher.Options.TargetDatabase.Name,
                            RootItem = publisher.Options.RootItem
                        };
                    }
                }
            }

            var remoteEventArgs = args as PublishEndRemoteEventArgs;
            if (remoteEventArgs != null)
            {
                #region modified part of the code - instead of using remoteEventArgs.TargetDatabaseName use value of the "Sitecore.Support.CommercePublishCacheRefresh.TargetDatabase" setting or "web" as a default value

                string targetDatabase = Settings.GetSetting("Sitecore.Support.CommercePublishCacheRefresh.TargetDatabase", "web");

                var database =
                    Factory.GetDatabase(targetDatabase);


                var rootItem =
                    database.GetItem(new ID(remoteEventArgs.RootItemId));

                return new ItemPublishOptions
                {
                    TargetDatabase = database,
                    TargetDatabaseName = targetDatabase,
                    RootItem = rootItem
                };
                #endregion
            }

            return null;
        }
    }

    public class ItemPublishOptions
    {
        public Database TargetDatabase { get; set; }

        public string TargetDatabaseName { get; set; }

        public Item RootItem { get; set; }
    }
}
