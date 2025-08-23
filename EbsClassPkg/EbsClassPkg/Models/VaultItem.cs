using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EbsClassPkg.Models {
    public class VaultItem {
        public int ID { get; set; }
        public bool IsFile { get; set; }
        public bool IsAdminLevel { get; set; }
        public string Name { get; set; }
        public string Name_Hierarchical { get; set; }
        public string VaultURL { get; set; }
        public int CompID { get; set; }
        public int ClientID { get; set; }
        public int ParentVaultItemID { get; set; }
        public int RootParentVaultItemID { get; set; }
        public string CssClasses { get; set; }
        public string FileExt { get; set; }
        public bool IsRootLvl { get; set; }
        public bool IsSysGenerated { get; set; }
    }
}