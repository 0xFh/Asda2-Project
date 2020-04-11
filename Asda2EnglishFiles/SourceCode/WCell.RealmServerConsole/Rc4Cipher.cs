// Decompiled with JetBrains decompiler
// Type: Rc4Cipher
// Assembly: WCell.RealmServerConsole, Version=0.5.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1D19BA74-5B0A-4712-9872-8B8B01DD7A49
// Assembly location: C:\Users\NoN\Desktop\Run\Debug\WCell.RealmServerConsole.exe

using System;

public class Rc4Cipher
{
    private Rc4Cipher.RC4 serverCrypt = (Rc4Cipher.RC4) null;

    public void initialize_ServerCrypto(byte[] m_CMKEYLoginHashMD5)
    {
        this.serverCrypt = new Rc4Cipher.RC4(m_CMKEYLoginHashMD5);
    }

    public void server_deCrypt(byte[] rawData)
    {
        this.serverCrypt.decrypt(rawData);
    }

    public void server_deCrypt_reset(byte[] rawData)
    {
        this.serverCrypt.decrypt_reset(rawData);
    }

    private class RC4
    {
        private int x = 0;
        private int y = 0;
        private byte[] state = new byte[256];

        public RC4(byte[] key)
        {
            for (int index = 0; index < 256; ++index)
                this.state[index] = (byte) index;
            int j = 0;
            for (int i = 0; i < 256; ++i)
            {
                j = j + (int) key[i % key.Length] + (int) this.state[i] & (int) byte.MaxValue;
                this.swap(i, j);
            }
        }

        public void decrypt(byte[] b)
        {
            for (int index = 0; index < b.Length; ++index)
                b[index] = (byte) (((int) b[index] ^ this.rc4()) & (int) byte.MaxValue);
        }

        public void decrypt_reset(byte[] b)
        {
            int x = this.x;
            int y = this.y;
            byte[] numArray = new byte[this.state.Length];
            this.state.CopyTo((Array) numArray, 0);
            for (int index = 0; index < b.Length; ++index)
                b[index] = (byte) (((int) b[index] ^ this.rc4()) & (int) byte.MaxValue);
            this.x = x;
            this.y = y;
            this.state = numArray;
        }

        private void swap(int i, int j)
        {
            byte num = this.state[i];
            this.state[i] = this.state[j];
            this.state[j] = num;
        }

        private int rc4()
        {
            this.x = this.x + 1 & (int) byte.MaxValue;
            this.y = this.y + (int) this.state[this.x] & (int) byte.MaxValue;
            this.swap(this.x, this.y);
            return (int) this.state[(int) this.state[this.x] + (int) this.state[this.y] & (int) byte.MaxValue];
        }
    }
}