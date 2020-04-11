namespace WCell.Core.Cryptography
{
    public class ARC4
    {
        private readonly byte[] state;
        private byte x;
        private byte y;

        public ARC4(byte[] key)
        {
            this.state = new byte[256];
            this.x = this.y = (byte) 0;
            this.KeySetup(key);
        }

        public int Process(byte[] buffer, int start, int count)
        {
            return this.InternalTransformBlock(buffer, start, count, buffer, start);
        }

        private void KeySetup(byte[] key)
        {
            byte num1 = 0;
            byte num2 = 0;
            for (int index = 0; index < 256; ++index)
                this.state[index] = (byte) index;
            this.x = (byte) 0;
            this.y = (byte) 0;
            for (int index = 0; index < 256; ++index)
            {
                num2 = (byte) ((uint) key[(int) num1] + (uint) this.state[index] + (uint) num2);
                byte num3 = this.state[index];
                this.state[index] = this.state[(int) num2];
                this.state[(int) num2] = num3;
                num1 = (byte) (((int) num1 + 1) % key.Length);
            }
        }

        private int InternalTransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer,
            int outputOffset)
        {
            for (int index = 0; index < inputCount; ++index)
            {
                ++this.x;
                this.y = (byte) ((uint) this.state[(int) this.x] + (uint) this.y);
                byte num1 = this.state[(int) this.x];
                this.state[(int) this.x] = this.state[(int) this.y];
                this.state[(int) this.y] = num1;
                byte num2 = (byte) ((uint) this.state[(int) this.x] + (uint) this.state[(int) this.y]);
                outputBuffer[outputOffset + index] =
                    (byte) ((uint) inputBuffer[inputOffset + index] ^ (uint) this.state[(int) num2]);
            }

            return inputCount;
        }
    }
}