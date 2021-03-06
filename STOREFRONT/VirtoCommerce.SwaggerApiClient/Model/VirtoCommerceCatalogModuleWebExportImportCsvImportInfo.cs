using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;



namespace VirtoCommerce.SwaggerApiClient.Model {

  /// <summary>
  /// 
  /// </summary>
  [DataContract]
  public class VirtoCommerceCatalogModuleWebExportImportCsvImportInfo {
    
    /// <summary>
    /// Gets or Sets CatalogId
    /// </summary>
    [DataMember(Name="catalogId", EmitDefaultValue=false)]
    public string CatalogId { get; set; }

    
    /// <summary>
    /// Gets or Sets FileUrl
    /// </summary>
    [DataMember(Name="fileUrl", EmitDefaultValue=false)]
    public string FileUrl { get; set; }

    
    /// <summary>
    /// Gets or Sets Configuration
    /// </summary>
    [DataMember(Name="configuration", EmitDefaultValue=false)]
    public VirtoCommerceCatalogModuleWebExportImportCsvProductMappingConfiguration Configuration { get; set; }

    

    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class VirtoCommerceCatalogModuleWebExportImportCsvImportInfo {\n");
      
      sb.Append("  CatalogId: ").Append(CatalogId).Append("\n");
      
      sb.Append("  FileUrl: ").Append(FileUrl).Append("\n");
      
      sb.Append("  Configuration: ").Append(Configuration).Append("\n");
      
      sb.Append("}\n");
      return sb.ToString();
    }

    /// <summary>
    /// Get the JSON string presentation of the object
    /// </summary>
    /// <returns>JSON string presentation of the object</returns>
    public string ToJson() {
      return JsonConvert.SerializeObject(this, Formatting.Indented);
    }

}


}
