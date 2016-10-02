using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace MyNetShopProject.Features.RegisterWebConfigModificationsFeature
{
    [Guid("47da747d-7528-4de6-be98-9439ab0d1b11")]
    public class RegisterWebConfigModificationsFeatureEventReceiver : SPFeatureReceiver
    {
        private readonly IReadOnlyCollection<SPWebConfigModification> _modifications;

        private readonly string _modificationOwner =
            typeof (RegisterWebConfigModificationsFeatureEventReceiver).FullName;

        public RegisterWebConfigModificationsFeatureEventReceiver()
        {
            _modifications = new ReadOnlyCollection<SPWebConfigModification>(new[]
            {
                CreateWebConfigModification("configuration/system.web/httpModules"),
                CreateWebConfigModification("configuration/system.web/httpHandlers",
                    "verb='GET' path='DX.ashx' validate='false' "),
                CreateWebConfigModification("configuration/system.webServer/modules"),
                CreateWebConfigModification("configuration/system.webServer/handlers",
                    "verb='GET' path='DX.ashx' preCondition='integratedMode' ")
            });
        }

        public IReadOnlyCollection<SPWebConfigModification> Modifications
        {
            get { return _modifications; }
        }

        public string ModificationOwner
        {
            get { return _modificationOwner; }
        }

        private SPWebConfigModification CreateWebConfigModification(
            string path, 
            string additions = "")
        {
            const string devexpressTypeName =
                "DevExpress.Web.ASPxHttpHandlerModule, " +
                "DevExpress.Web.v15.2, Version=15.2.7.0, Culture=neutral, " +
                "PublicKeyToken=b88d1754d700e49a";

            return new SPWebConfigModification
            {
                Path = path,
                Owner = _modificationOwner,
                Name = string.Format("add[@name='{0}']", _modificationOwner),
                Type = SPWebConfigModification.SPWebConfigModificationType.EnsureChildNode,
                Value = string.Format("<add name='{0}' type='{1}' {2} />",
                                      _modificationOwner, devexpressTypeName, additions)
            };
        }

        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {
            var webApplication = (SPWebApplication)properties.Feature.Parent;
            AddWebConfigModifications(webApplication, _modifications);
        }

        public override void FeatureDeactivating(SPFeatureReceiverProperties properties)
        {
            var webApplication = (SPWebApplication)properties.Feature.Parent;
            RemoveWebConfigModificationsByOwner(webApplication, _modificationOwner);
        }

        public static void AddWebConfigModifications(
            SPWebApplication webApplication,
            IEnumerable<SPWebConfigModification> modifications)
        {
            if (webApplication == null)
                throw new ArgumentNullException("webApplication");
            if (modifications == null)
                throw new ArgumentNullException("modifications");

            foreach (var modification in modifications)
                webApplication.WebConfigModifications.Add(modification);

            ApplyWebConfigModifications(webApplication);
        }

        public static void RemoveWebConfigModificationsByOwner(
            SPWebApplication webApplication,
            string owner)
        {
            if (webApplication == null)
                throw new ArgumentNullException("webApplication");
            if (owner == null)
                throw new ArgumentNullException("owner");

            var toRemove = webApplication.WebConfigModifications
                                         .Where(m => m.Owner == owner)
                                         .ToList();
            foreach (var modification in toRemove)
                webApplication.WebConfigModifications.Remove(modification);

            ApplyWebConfigModifications(webApplication);
        }

        public static void ApplyWebConfigModifications(
           SPWebApplication webApplication)
        {
            if (webApplication == null)
                throw new ArgumentNullException("webApplication");

            webApplication.Update();
            webApplication.WebService.ApplyWebConfigModifications();
        }
    }
}
