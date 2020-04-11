using System;
using System.Collections.Generic;
using System.Text;

public class Rc4Cipher
    {
        private RC4 serverCrypt = null;

        public  void initialize_ServerCrypto(byte[] m_CMKEYLoginHashMD5)
        {
            serverCrypt = new RC4(m_CMKEYLoginHashMD5);
        }


        public  void server_deCrypt(byte[] rawData)
        {
            serverCrypt.decrypt(rawData);
        }

        public void server_deCrypt_reset(byte[] rawData)
        {
            serverCrypt.decrypt_reset(rawData);
        }

        private class RC4
        {
            private int x = 0;
            private int y = 0;
            private byte[] state = new byte[256];

            public RC4(byte[] key)
            {
                for (int i = 0; i < 256; i++)
                    state[i] = (byte)i;

                int j = 0;
                for (int i = 0; i < 256; i++)
                {
                    j = (j + key[i % key.Length] + state[i]) & 255;
                    swap(i, j);
                }
            }

            public void decrypt(byte[] b)
            {
                for (int i = 0; i < b.Length; i++)
                    b[i] = (byte)((b[i] ^ rc4()) & 255);
            }

            public void decrypt_reset(byte[] b)
            {
                int a = x;
                int c = y;
                byte[] temp = new byte[state.Length];
                state.CopyTo(temp,0);
                for (int i = 0; i < b.Length; i++)
                    b[i] = (byte)((b[i] ^ rc4()) & 255);
                x = a;
                y = c;
                state = temp;
            }

            private void swap(int i, int j)
            {
                byte temp = state[i];
                state[i] = state[j];
                state[j] = temp;
            }

            private int rc4()
            {
                x = (x + 1) & 255;
                y = (y + state[x]) & 255;

                swap(x, y);

                return state[(state[x] + state[y]) & 255];
            }
        }

    }



