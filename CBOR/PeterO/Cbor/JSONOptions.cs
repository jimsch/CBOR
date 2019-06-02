using System;
using System.Collections.Generic;
using System.Text;

namespace PeterO.Cbor {
    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="T:PeterO.Cbor.JSONOptions"]/*'/>
    public sealed class JSONOptions {
    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Cbor.JSONOptions.#ctor"]/*'/>
    public JSONOptions() : this(false) {
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Cbor.JSONOptions.#ctor(System.Boolean)"]/*'/>
    public JSONOptions(bool base64Padding) : this(base64Padding, false) {
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Cbor.JSONOptions.#ctor(System.Boolean,System.Boolean)"]/*'/>
    public JSONOptions(bool base64Padding, bool replaceSurrogates) {
        this.Base64Padding = base64Padding;
        this.ReplaceSurrogates = replaceSurrogates;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="F:PeterO.Cbor.JSONOptions.Default"]/*'/>
    public static readonly JSONOptions Default = new JSONOptions();

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="P:PeterO.Cbor.JSONOptions.Base64Padding"]/*'/>
  [Obsolete("This option may have no effect in the future. A future version may, by default, include necessary padding when writing traditional base64 to JSON and include no padding when writing base64url to JSON, in accordance with the revision of the CBOR specification.")]
    public bool Base64Padding { get; private set; }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="P:PeterO.Cbor.JSONOptions.ReplaceSurrogates"]/*'/>
    public bool ReplaceSurrogates { get; private set; }
   }
}
