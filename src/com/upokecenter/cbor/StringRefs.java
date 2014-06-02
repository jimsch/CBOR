package com.upokecenter.cbor;
/*
Written by Peter O. in 2014.
Any copyright is dedicated to the Public Domain.
http://creativecommons.org/publicdomain/zero/1.0/
If you like this, you should donate to Peter O.
at: http://upokecenter.com/d/
 */

import java.util.*;
import com.upokecenter.util.*;

    /**
     * Implements CBOR string references, described at <code>http://cbor.schmorp.de/stringref</code>
     */
  class StringRefs
  {
    private ArrayList<ArrayList<CBORObject>> stack;

    public StringRefs () {
      this.stack = new ArrayList<ArrayList<CBORObject>>();
      ArrayList<CBORObject> firstItem=new ArrayList<CBORObject>();
      this.stack.add(firstItem);
    }

    public void Push() {
      ArrayList<CBORObject> firstItem=new ArrayList<CBORObject>();
      this.stack.add(firstItem);
    }

    public void Pop() {

      this.stack.remove(this.stack.size() - 1);
    }

    public void AddStringIfNeeded(CBORObject str, int lengthHint) {

      boolean addStr = false;
      ArrayList<CBORObject> lastList = this.stack.get(this.stack.size() - 1);
      if (lastList.size() < 24) {
        if (lengthHint >= 3) {
          addStr = true;
        }
      } else if (lastList.size() < 256) {
        if (lengthHint >= 4) {
          addStr = true;
        }
      } else if (lastList.size() < 65536) {
        if (lengthHint >= 5) {
          addStr = true;
        }
      } else {
        // NOTE: lastList's size can't be higher than (2^64)-1
        if (lengthHint >= 7) {
          addStr = true;
        }
      }
      // NOTE: An additional branch, with lengthHint >= 11, would
      // be needed if the size could be higher than (2^64)-1
      if (addStr) {
        lastList.add(str);
      }
    }

    public CBORObject GetString(long smallIndex) {
      if (smallIndex < 0) {
        throw new CBORException("Unexpected index");
      }
      if (smallIndex > Integer.MAX_VALUE) {
        throw new CBORException("Index " + smallIndex + " is bigger than supported");
      }
      int index = (int)smallIndex;
      ArrayList<CBORObject> lastList = this.stack.get(this.stack.size() - 1);
      if (index >= lastList.size()) {
        throw new CBORException("Index " + index + " is not valid");
      }
      CBORObject ret = lastList.get(index);
      if (ret.getType() == CBORType.ByteString) {
        // Byte strings are mutable, so make a copy
        return CBORObject.FromObject(ret.GetByteString());
      } else {
        return ret;
      }
    }

    public CBORObject GetString(BigInteger bigIndex) {
      if (bigIndex.signum() < 0) {
        throw new CBORException("Unexpected index");
      }
      if (!bigIndex.canFitInInt()) {
        throw new CBORException("Index " + bigIndex + " is bigger than supported");
      }
      int index = bigIndex.intValue();
      ArrayList<CBORObject> lastList = this.stack.get(this.stack.size() - 1);
      if (index >= lastList.size()) {
        throw new CBORException("Index " + index + " is not valid");
      }
      CBORObject ret = lastList.get(index);
      if (ret.getType() == CBORType.ByteString) {
        // Byte strings are mutable, so make a copy
        return CBORObject.FromObject(ret.GetByteString());
      } else {
        return ret;
      }
    }
  }
