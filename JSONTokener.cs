/*
// Modified by Peter O; originally based on the
// 2002 public domain
// code from json.org, to use generics and
// to use int and -1 as the terminating
// value rather than char and 0, among
// other things.
// Now much of this file has been rewritten and
// altered by Peter O. to support the CBOR project.
// Still in the public domain;
// public domain dedication: http://creativecommons.org/publicdomain/zero/1.0/
 */
namespace PeterO {
  using System;
  using System.Collections.Generic;
  using System.Globalization;
  using System.IO;
  using System.Text;

  internal class JSONTokener {
    /**
     * The index of the next character.
     */
    private int myIndex;
    /**
     * The source string being tokenized.
     */
    private string mySource;
    private Stream stream;
    /**
     * Construct a JSONTokener from a string.
     *
     * @param s A source _string.
     */
    public JSONTokener(string str) {
      if (str == null) {
        throw new ArgumentNullException("str");
      }
      this.mySource = str;
    }

    public JSONTokener(Stream stream) {
      if (stream == null) {
        throw new ArgumentNullException("stream");
      }
      this.stream = stream;
    }

    /// <summary>
    /// Reads the next character from a UTF-8 stream or a string.
    /// </summary>
    /// <returns>The next character, or -1 if the end of the
    /// string or stream was reached.</returns>
    public int NextChar() {
      if (this.stream != null) {
        int cp = 0;
        int bytesSeen = 0;
        int bytesNeeded = 0;
        int lower = 0;
        int upper = 0;
        try {
          while (true) {
            int b = this.stream.ReadByte();
            if (b < 0) {
              if (bytesNeeded != 0) {
                bytesNeeded = 0;
                throw this.SyntaxError("Invalid UTF-8");
              } else {
                return -1;
              }
            }
            if (bytesNeeded == 0) {
              if ((b & 0x7F) == b) {
                ++this.myIndex;
                return b;
              } else if (b >= 0xc2 && b <= 0xdf) {
                bytesNeeded = 1;
                lower = 0x80;
                upper = 0xbf;
                cp = (b - 0xc0) << 6;
              } else if (b >= 0xe0 && b <= 0xef) {
                lower = (b == 0xe0) ? 0xa0 : 0x80;
                upper = (b == 0xed) ? 0x9f : 0xbf;
                bytesNeeded = 2;
                cp = (b - 0xe0) << 12;
              } else if (b >= 0xf0 && b <= 0xf4) {
                lower = (b == 0xf0) ? 0x90 : 0x80;
                upper = (b == 0xf4) ? 0x8f : 0xbf;
                bytesNeeded = 3;
                cp = (b - 0xf0) << 18;
              } else {
                throw this.SyntaxError("Invalid UTF-8");
              }
              continue;
            }
            if (b < lower || b > upper) {
              cp = bytesNeeded = bytesSeen = 0;
              lower = 0x80;
              upper = 0xbf;
              throw this.SyntaxError("Invalid UTF-8");
            }
            lower = 0x80;
            upper = 0xbf;
            ++bytesSeen;
            cp += (b - 0x80) << (6 * (bytesNeeded - bytesSeen));
            if (bytesSeen != bytesNeeded) {
              continue;
            }
            int ret = cp;
            cp = 0;
            bytesSeen = 0;
            bytesNeeded = 0;
            ++this.myIndex;
            return ret;
          }
        } catch (IOException ex) {
          throw this.SyntaxError("I/O error occurred", ex);
        }
      } else {
        int c = (this.myIndex < this.mySource.Length) ? this.mySource[this.myIndex] : -1;
        if (c >= 0xD800 && c <= 0xDBFF && this.myIndex + 1 < this.mySource.Length &&
            this.mySource[this.myIndex + 1] >= 0xDC00 && this.mySource[this.myIndex + 1] <= 0xDFFF) {
          // Get the Unicode code point for the surrogate pair
          c = 0x10000 + ((c - 0xD800) * 0x400) + (this.mySource[this.myIndex + 1] - 0xDC00);
          ++this.myIndex;
        } else if (c >= 0xD800 && c <= 0xDFFF) {
          // unpaired surrogate
          throw this.SyntaxError("Unpaired surrogate code point");
        }
        ++this.myIndex;
        return c;
      }
    }

    /// <summary>Not documented yet.</summary>
    /// <returns>A 32-bit signed integer.</returns>
    public int NextSyntaxChar() {
      while (true) {
        int c = this.NextChar();
        if (c == -1 || (c != 0x20 && c != 0x0a && c != 0x0d && c != 0x09)) {
          return c;
        }
      }
    }

    /// <summary>Not documented yet.</summary>
    /// <param name='lastChar'>A 32-bit signed integer. (2).</param>
    /// <returns>A 32-bit signed integer.</returns>
    private int NextSyntaxChar2(int lastChar) {
      while (true) {
        if (lastChar == '/' || lastChar == '#') {
          throw this.SyntaxError("Comments not allowed");
        }
        int c = (lastChar >= 0) ? lastChar : this.NextChar();
        if (c == -1 || (c != 0x20 && c != 0x0a && c != 0x0d && c != 0x09)) {
          return c;
        }
        lastChar = -1;
      }
    }
    /**
     * Return the characters up to the next close quote character.
     * Backslash processing is done.
     * @param quote The quoting character.
     * @return A string.
     * @exception FormatException Unterminated _string.
     */
    private string NextString(int quote) {
      int c;
      StringBuilder sb = new StringBuilder();
      bool surrogate = false;
      bool surrogateEscaped = false;
      bool escaped = false;
      while (true) {
        c = this.NextChar();
        if (c == -1 || c < 0x20) {
          throw this.SyntaxError("Unterminated string");
        }
        switch (c) {
          case '\\':
            c = this.NextChar();
            escaped = true;
            switch (c) {
              case '\\':
                c = '\\';
                break;
              case '/':
                // Not allowed to be escaped by RFC 4627,
                // but will be allowed in the revision to RFC 4627
                c = '/';
                break;
              case '\"':
                c = '\"';
                break;
              case 'b':
                c = '\b';
                break;
              case 'f':
                c = '\f';
                break;
              case 'n':
                c = '\n';
                break;
              case 'r':
                c = '\r';
                break;
              case 't':
                c = '\t';
                break;
                case 'u': { // Unicode escape
                  c = 0;
                  // Consists of 4 hex digits
                  for (int i = 0; i < 4; ++i) {
                    int ch = this.NextChar();
                    if (ch >= '0' && ch <= '9') {
                      c <<= 4;
                      c |= ch - '0';
                    } else if (ch >= 'A' && ch <= 'F') {
                      c <<= 4;
                      c |= ch + 10 - 'A';
                    } else if (ch >= 'a' && ch <= 'f') {
                      c <<= 4;
                      c |= ch + 10 - 'a';
                    } else {
                      throw this.SyntaxError("Invalid Unicode escaped character");
                    }
                  }
                  break;
                }
              default:
                throw this.SyntaxError("Invalid escaped character");
            }
            break;
          default:
            escaped = false;
            break;
        }
        if (surrogate) {
          if ((c & 0x1FFC00) != 0xDC00) {
            // Note: this includes the ending quote
            // and supplementary characters
            throw this.SyntaxError("Unpaired surrogate code point");
          }
          if (escaped != surrogateEscaped) {
            throw this.SyntaxError("Pairing escaped surrogate with unescaped surrogate");
          }
          surrogate = false;
        } else if ((c & 0x1FFC00) == 0xD800) {
          surrogate = true;
          surrogateEscaped = escaped;
        } else if ((c & 0x1FFC00) == 0xDC00) {
          throw this.SyntaxError("Unpaired surrogate code point");
        }
        if (c == quote && !escaped) {
          // End quote reached
          return sb.ToString();
        }
        if (c <= 0xFFFF) {
          sb.Append((char)c);
        } else if (c <= 0x10FFFF) {
          sb.Append((char)((((c - 0x10000) >> 10) & 0x3FF) + 0xD800));
          sb.Append((char)(((c - 0x10000) & 0x3FF) + 0xDC00));
        }
      }
    }

    internal CBORException SyntaxError(string message) {
      return new CBORException(message + this.ToString() + "(char. " + this.myIndex + ")");
    }

    internal CBORException SyntaxError(string message, Exception innerException) {
      return new CBORException(message + this.ToString() + "(char. " + this.myIndex + ")", innerException);
    }

    private CBORObject NextJSONValue(int firstChar, bool noDuplicates, int[] nextChar) {
      string str;
      int c = firstChar;
      CBORObject obj = null;
      if (c < 0) {
        throw this.SyntaxError("Unexpected end of data");
      }
      if (c == '"') {
        // Parse a string
        // The tokenizer already checked the string for invalid
        // surrogate pairs, so just call the CBORObject
        // constructor directly
        obj = CBORObject.FromRaw(this.NextString(c));
        nextChar[0] = this.NextSyntaxChar();
        return obj;
      } else if (c == '{') {
        // Parse an object
        obj = this.ParseJSONObject(noDuplicates);
        nextChar[0] = this.NextSyntaxChar();
        return obj;
      } else if (c == '[') {
        // Parse an array
        obj = this.ParseJSONArray(noDuplicates);
        nextChar[0] = this.NextSyntaxChar();
        return obj;
      } else if (c == 't') {
        // Parse true
        if (this.NextChar() != 'r' ||
            this.NextChar() != 'u' ||
            this.NextChar() != 'e') {
          throw this.SyntaxError("Value can't be parsed.");
        }
        nextChar[0] = this.NextSyntaxChar();
        return CBORObject.True;
      } else if (c == 'f') {
        // Parse false
        if (this.NextChar() != 'a' ||
            this.NextChar() != 'l' ||
            this.NextChar() != 's' ||
            this.NextChar() != 'e') {
          throw this.SyntaxError("Value can't be parsed.");
        }
        nextChar[0] = this.NextSyntaxChar();
        return CBORObject.False;
      } else if (c == 'n') {
        // Parse null
        if (this.NextChar() != 'u' ||
            this.NextChar() != 'l' ||
            this.NextChar() != 'l') {
          throw this.SyntaxError("Value can't be parsed.");
        }
        nextChar[0] = this.NextSyntaxChar();
        return CBORObject.Null;
      } else if (c == '-' || (c >= '0' && c <= '9')) {
        // Parse a number
        StringBuilder sb = new StringBuilder();
        while (c == '-' || c == '+' || c == '.' || c == 'e' || c == 'E' || (c >= '0' && c <= '9')) {
          sb.Append((char)c);
          c = this.NextChar();
        }
        str = sb.ToString();
        obj = CBORDataUtilities.ParseJSONNumber(str);
        if (obj == null) {
          throw this.SyntaxError("JSON number can't be parsed.");
        }
        nextChar[0] = this.NextSyntaxChar2(c);
        return obj;
      } else {
        throw this.SyntaxError("Value can't be parsed.");
      }
    }

    /// <summary>Not documented yet.</summary>
    /// <returns>A CBORObject object.</returns>
    /// <param name='noDuplicates'>A Boolean object.</param>
    public CBORObject ParseJSONObjectOrArray(bool noDuplicates) {
      int c;
      c = this.NextSyntaxChar();
      if (c == '[') {
        return this.ParseJSONArray(noDuplicates);
      }
      if (c == '{') {
        return this.ParseJSONObject(noDuplicates);
      }
      throw this.SyntaxError("A JSON object must begin with '{' or '['");
    }

    private CBORObject ParseJSONObject(bool noDuplicates) {
      // Assumes that the last character read was '{'
      int c;
      CBORObject key;
      CBORObject obj;
      int[] nextchar = new int[1];
      bool seenComma = false;
      var myHashMap = new Dictionary<CBORObject, CBORObject>();
      while (true) {
        c = this.NextSyntaxChar();
        switch (c) {
          case -1:
            throw this.SyntaxError("A JSONObject must end with '}'");
          case '}':
            if (seenComma) {
              // Situation like '{"0"=>1,}'
              throw this.SyntaxError("Trailing comma");
            }
            return CBORObject.FromRaw(myHashMap);
            default: {
              // Read the next string
              if (c < 0) {
                throw this.SyntaxError("Unexpected end of data");
              }
              if (c != '"') {
                throw this.SyntaxError("Expected a string as a key");
              }
              // Parse a string that represents the object's key
              // The tokenizer already checked the string for invalid
              // surrogate pairs, so just call the CBORObject
              // constructor directly
              obj = CBORObject.FromRaw(this.NextString(c));
              key = obj;
              if (noDuplicates && myHashMap.ContainsKey(obj)) {
                throw this.SyntaxError("Key already exists: " + key);
              }
              break;
            }
        }
        if (this.NextSyntaxChar() != ':') {
          throw this.SyntaxError("Expected a ':' after a key");
        }
        // NOTE: Will overwrite existing value. --Peter O.
        myHashMap[key] = this.NextJSONValue(this.NextSyntaxChar(), noDuplicates, nextchar);
        switch (nextchar[0]) {
          case ',':
            seenComma = true;
            break;
          case '}':
            return CBORObject.FromRaw(myHashMap);
          default:
            throw this.SyntaxError("Expected a ',' or '}'");
        }
      }
    }

    private CBORObject ParseJSONArray(bool noDuplicates) {
      var myArrayList = new List<CBORObject>();
      bool seenComma = false;
      int[] nextchar = new int[1];
      // This method assumes that the last character read was '['
      while (true) {
        int c = this.NextSyntaxChar();
        if (c == ',') {
          // Situation like '[,0,1,2]' or '[0,,1]'
          throw this.SyntaxError("Empty array element");
        } else if (c == ']') {
          if (seenComma) {
            // Situation like '[0,1,]'
            throw this.SyntaxError("Trailing comma");
          }
          return CBORObject.FromRaw(myArrayList);
        } else {
          myArrayList.Add(this.NextJSONValue(c, noDuplicates, nextchar));
          c = nextchar[0];
        }
        switch (c) {
          case ',':
            seenComma = true;
            break;
          case ']':
            return CBORObject.FromRaw(myArrayList);
          default:
            throw this.SyntaxError("Expected a ',' or ']'");
        }
      }
    }
  }
}
