//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DB
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    
    public partial class Source
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Source()
        {
            this.Targets = new HashSet<Target>();
        }
    
        public int id { get; set; }
        public string sourceVMResourceGroup { get; set; }
        public string sourceVMName { get; set; }
        public string imagesLocation { get; set; }
        public string imagesResourceGroup { get; set; }
        public string imagePrefix { get; set; }
        public string imageVersion { get; set; }
        public string status { get; set; }
        public Nullable<System.DateTime> imageDate { get; set; }
        public int imageRecurrance { get; set; }
    
        [JsonIgnoreAttribute]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Target> Targets { get; set; }
    }
}