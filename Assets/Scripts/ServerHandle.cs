using UnityEngine;

public class ServerHandle
{
    public static void WelcomeReceived(int _fromClient, Packet _packet)
    {
        int _clientIdCheck = _packet.ReadInt();
        string _username = _packet.ReadString();

        Debug.Log($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully and is now player {_fromClient}.");
        Server.CurrentPlayers++;
        Debug.Log(Server.CurrentPlayers);
        if (_fromClient != _clientIdCheck)
        {
            Debug.Log($"Player \"{_username}\" (ID: {_fromClient}) has assumed the wrong client ID ({_clientIdCheck})!");
        }
        Server.clients[_fromClient].SendIntoGame(_username);
    }

    public static void PlayerMovement(int _fromClient, Packet _packet)
    {
        bool[] _inputs = new bool[_packet.ReadInt()];
        for (int i = 0; i < _inputs.Length; i++)
        {
            _inputs[i] = _packet.ReadBool();
        }
        Quaternion _rotation = _packet.ReadQuaternion();

        Server.clients[_fromClient].rbplayer.SetInput(_inputs, _rotation);
    }

    public static void BasicAttack(int _fromClient, Packet _packet)
    {
        Vector3 shootDirection = _packet.ReadVector3();
        Server.clients[_fromClient].rbplayer.BasicAttack(shootDirection);
        Debug.Log($"{_fromClient} basic");
    }

    public static void Spell1(int _fromClient, Packet _packet)
    {
        Vector3 _throwDirection = _packet.ReadVector3();
        Server.clients[_fromClient].rbplayer.Spell1(_throwDirection);
        Debug.Log($"{_fromClient} spell1");
    }

    public static void Spell2(int _fromClient, Packet _packet)
    {
        Server.clients[_fromClient].rbplayer.Spell2();
        Debug.Log($"{_fromClient} spell2");
    }

    public static void SpecialAttack(int _fromClient, Packet _packet)
    {
        Vector3 _direction = _packet.ReadVector3();
        Server.clients[_fromClient].rbplayer.SpecialAttack(_direction);
        Debug.Log($"{_fromClient} special");
    }
}

