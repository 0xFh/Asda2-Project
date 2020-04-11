using System;

public class Rc4Cipher
{
  private RC4 serverCrypt;

  public void initialize_ServerCrypto(byte[] m_CMKEYLoginHashMD5)
  {
    serverCrypt = new RC4(m_CMKEYLoginHashMD5);
  }

  public void server_deCrypt(byte[] rawData)
  {
    serverCrypt.decrypt(rawData);
  }

  public void server_deCrypt_reset(byte[] rawData)
  {
    serverCrypt.decrypt_reset(rawData);
  }

  private class RC4
  {
    private int x;
    private int y;
    private byte[] state = new byte[256];

    public RC4(byte[] key)
    {
      for(int index = 0; index < 256; ++index)
        state[index] = (byte) index;
      int j = 0;
      for(int i = 0; i < 256; ++i)
      {
        j = j + key[i % key.Length] + state[i] & byte.MaxValue;
        swap(i, j);
      }
    }

    public void decrypt(byte[] b)
    {
      for(int index = 0; index < b.Length; ++index)
        b[index] = (byte) ((b[index] ^ rc4()) & byte.MaxValue);
    }

    public void decrypt_reset(byte[] b)
    {
      int x = this.x;
      int y = this.y;
      byte[] numArray = new byte[state.Length];
      state.CopyTo(numArray, 0);
      for(int index = 0; index < b.Length; ++index)
        b[index] = (byte) ((b[index] ^ rc4()) & byte.MaxValue);
      this.x = x;
      this.y = y;
      state = numArray;
    }

    private void swap(int i, int j)
    {
      byte num = state[i];
      state[i] = state[j];
      state[j] = num;
    }

    private int rc4()
    {
      x = x + 1 & byte.MaxValue;
      y = y + state[x] & byte.MaxValue;
      swap(x, y);
      return state[state[x] + state[y] & byte.MaxValue];
    }
  }
}