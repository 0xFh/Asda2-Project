namespace WCell.Core.Cryptography
{
  public class ARC4
  {
    private readonly byte[] state;
    private byte x;
    private byte y;

    public ARC4(byte[] key)
    {
      state = new byte[256];
      x = y = 0;
      KeySetup(key);
    }

    public int Process(byte[] buffer, int start, int count)
    {
      return InternalTransformBlock(buffer, start, count, buffer, start);
    }

    private void KeySetup(byte[] key)
    {
      byte num1 = 0;
      byte num2 = 0;
      for(int index = 0; index < 256; ++index)
        state[index] = (byte) index;
      x = 0;
      y = 0;
      for(int index = 0; index < 256; ++index)
      {
        num2 = (byte) (key[num1] + (uint) state[index] + num2);
        byte num3 = state[index];
        state[index] = state[num2];
        state[num2] = num3;
        num1 = (byte) ((num1 + 1) % key.Length);
      }
    }

    private int InternalTransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer,
      int outputOffset)
    {
      for(int index = 0; index < inputCount; ++index)
      {
        ++x;
        y = (byte) (state[x] + (uint) y);
        byte num1 = state[x];
        state[x] = state[y];
        state[y] = num1;
        byte num2 = (byte) (state[x] + (uint) state[y]);
        outputBuffer[outputOffset + index] =
          (byte) (inputBuffer[inputOffset + index] ^ (uint) state[num2]);
      }

      return inputCount;
    }
  }
}