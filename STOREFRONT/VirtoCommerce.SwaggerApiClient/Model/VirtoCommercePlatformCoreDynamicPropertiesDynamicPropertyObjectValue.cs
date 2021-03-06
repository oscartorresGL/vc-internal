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
  public class VirtoCommercePlatformCoreDynamicPropertiesDynamicPropertyObjectValue {
    
    /// <summary>
    /// Gets or Sets Locale
    /// </summary>
    [DataMember(Name="locale", EmitDefaultValue=false)]
    public string Locale { get; set; }

    
    /// <summary>
    /// Gets or Sets Value
    /// </summary>
    [DataMember(Name="value", EmitDefaultValue=false)]
    public Object Value { get; set; }

    

    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class VirtoCommercePlatformCoreDynamicPropertiesDynamicPropertyObjectValue {\n");
      
      sb.Append("  Locale: ").Append(Locale).Append("\n");
      
      sb.Append("  Value: ").Append(Value).Append("\n");
      
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
