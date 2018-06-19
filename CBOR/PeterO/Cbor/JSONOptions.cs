using System;
using System.Collections.Generic;
using System.Text;

namespace PeterO.Cbor {
    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="T:PeterO.Cbor.JSONOptions"]/*'/>
    public sealed class JSONOptions {
    /// <summary>Initializes a new instance of the JSONOptions class with
    /// default options.</summary>
    public JSONOptions() : this(false) {}

    /// <summary>Initializes a new instance of the JSONOptions class with
    /// the given values for the options.</summary>
    /// <param name='base64Padding'>Whether padding is included when
    /// writing data in base64url or traditional base64 format to
    /// JSON.</param>
    public JSONOptions(bool base64Padding) {
        this.Base64Padding = base64Padding;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="F:PeterO.Cbor.JSONOptions.Default"]/*'/>
    public readonly static JSONOptions Default = new JSONOptions();

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="P:PeterO.Cbor.JSONOptions.Base64Padding"]/*'/>
    public bool Base64Padding { get; private set; }
   }
}