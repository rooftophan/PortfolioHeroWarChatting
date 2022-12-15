using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Net;

public class ChattingPacket
{
    public const int PACKET_LENGTH_MAX = 2048;
    public short packetLength;
    public int bytesRead;
	public int bufPos;
	public byte[] packetBuffer;
	public byte[] backBuffer;

    public int curPacketLength;

    public ChattingPacket()
	{
        ClearPacket ();
	}

    public void ResetPacketBuffer(int length)
    {
        packetBuffer = new byte[length];
        System.Array.Clear(packetBuffer, 0, length);
    }

    public void ResetBackBuffer(int length)
    {
        backBuffer = new byte[length];
        System.Array.Clear(backBuffer, 0, length);
    }

    public void ClearPacket()
	{
        packetBuffer = new byte[PACKET_LENGTH_MAX];
        backBuffer = new byte[PACKET_LENGTH_MAX];
        System.Array.Clear(packetBuffer, 0, PACKET_LENGTH_MAX);
        System.Array.Clear(backBuffer, 0, PACKET_LENGTH_MAX);
        packetLength = 0;
        bytesRead = 0;
		bufPos = 0;

        curPacketLength = PACKET_LENGTH_MAX;
    }

	public void WriteHeader(ChattingPacketType chattingPacketType)
	{
		packetLength = 4;

		WriteShort(packetLength);		//Write PacketLength (default = 4)
		WriteShort((short)chattingPacketType);	//Write PacketNumber
	}
		
	public void WriteShort(short value)
	{
		if(System.BitConverter.IsLittleEndian)
			value = IPAddress.HostToNetworkOrder(value);

		System.Array.Copy(System.BitConverter.GetBytes(value), 0, packetBuffer, bufPos, sizeof(short));
		bufPos = bufPos + sizeof(short);
	}

	public void WriteInt(int value)
	{
		if(System.BitConverter.IsLittleEndian)
			value = IPAddress.HostToNetworkOrder(value);

		System.Array.Copy(System.BitConverter.GetBytes(value), 0, packetBuffer, bufPos, sizeof(int));
			
		bufPos = bufPos + sizeof(int);
	}
		
	public void WriteLong(long value)
	{
		if(System.BitConverter.IsLittleEndian)
			value = IPAddress.HostToNetworkOrder(value);
			
		System.Array.Copy(System.BitConverter.GetBytes(value), 0, packetBuffer, bufPos, sizeof(long));
			
		bufPos = bufPos + sizeof(long);
	}
		
	public void WriteMessage(string str)
	{
		byte[] encbuf = System.Text.Encoding.UTF8.GetBytes(str);
		string base64Str = System.Convert.ToBase64String(encbuf);
		encbuf = System.Text.ASCIIEncoding.ASCII.GetBytes(base64Str);

		//message buffer size
		WriteShort((short)encbuf.Length);

		//message buffer
		System.Array.Copy(encbuf, 0, packetBuffer, bufPos, sizeof(byte) * encbuf.Length);
		bufPos = bufPos + encbuf.Length;
	}

    public void WriteEncBuf(byte[] encbuf)
    {
        //message buffer size
        WriteShort((short)encbuf.Length);

        //message buffer
        System.Array.Copy(encbuf, 0, packetBuffer, bufPos, sizeof(byte) * encbuf.Length);
        bufPos = bufPos + encbuf.Length;
    }

    public short ReadShort()
	{
		short value = 0;
			
		value = System.BitConverter.ToInt16(packetBuffer, bufPos);
		if(System.BitConverter.IsLittleEndian)
			value = IPAddress.HostToNetworkOrder(value);
		bufPos = bufPos + sizeof(short);
			
		return value;
	}

	public short ReadShort(int currentPos)
	{
		short value = 0;
		
		value = System.BitConverter.ToInt16(packetBuffer, currentPos);
		if(System.BitConverter.IsLittleEndian)
			value = IPAddress.HostToNetworkOrder(value);

		return value;
	}
		
	public int ReadInt()
	{
		int value = 0;
			
		value = System.BitConverter.ToInt32(packetBuffer, bufPos);
		if(System.BitConverter.IsLittleEndian)
			value = IPAddress.HostToNetworkOrder(value);
			
		bufPos = bufPos + sizeof(int);
			
		return value;
	}
		
	public int ReadInt(int currentPos)
	{
		int value = 0;
		
		value = System.BitConverter.ToInt32(packetBuffer, currentPos);
		if(System.BitConverter.IsLittleEndian)
			value = IPAddress.HostToNetworkOrder(value);

		return value;
	}


	public long ReadLong()
	{
		long value = 0;
			
		value = System.BitConverter.ToInt64(packetBuffer, bufPos);
		if(System.BitConverter.IsLittleEndian)
			value = IPAddress.HostToNetworkOrder(value);
		bufPos = bufPos + sizeof(long);
			
		return value;
	}

	public long ReadLong(int currentPos)
	{
		long value = 0;
		
		value = System.BitConverter.ToInt64(packetBuffer, currentPos);
		if(System.BitConverter.IsLittleEndian)
			value = IPAddress.HostToNetworkOrder(value);

		return value;
	}

	public string ReadString(int readSize)
	{
		string base64Str;
		base64Str = System.Text.ASCIIEncoding.ASCII.GetString(packetBuffer, bufPos, readSize);
		bufPos = bufPos + readSize;
		byte[] decbuf = System.Convert.FromBase64String(base64Str);
		string msg = System.Text.Encoding.UTF8.GetString(decbuf);
		return msg;
	}

	public string ReadString(int readSize, int currentPos)
	{
		string base64Str;
		base64Str = System.Text.ASCIIEncoding.ASCII.GetString(packetBuffer, currentPos, readSize);
		byte[] decbuf = System.Convert.FromBase64String(base64Str);
		string msg = System.Text.Encoding.UTF8.GetString(decbuf);
		return msg;
	}
		
	public void MakePacket()
	{
		packetLength = (short)bufPos;
		bufPos = 0;
		WriteShort((short)packetLength);
	}
}




