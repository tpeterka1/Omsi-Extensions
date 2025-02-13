﻿namespace OmsiHook
{
    /// <summary>
    /// Base class for instances of Map Objects
    /// </summary>
    public class OmsiMapObjInst : OmsiObject
    {
        internal OmsiMapObjInst(Memory omsiMemory, int baseAddress) : base(omsiMemory, baseAddress) { }
        public OmsiMapObjInst() : base() { }

        public D3DVector Position
        {
            get => Memory.ReadMemory<D3DVector>(Address + 0x4);
            set => Memory.WriteMemory(Address + 0x4, value);
        }
        public D3DMatrix Pos_Mat
        {
            get => Memory.ReadMemory<D3DMatrix>(Address + 0x10);
            set => Memory.WriteMemory(Address + 0x10, value);
        }
        public D3DQuaternion Rotation
        {
            get => Memory.ReadMemory<D3DQuaternion>(Address + 0x50);
            set => Memory.WriteMemory(Address + 0x50, value);
        }
        public float Scale
        {
            get => Memory.ReadMemory<float>(Address + 0x60);
            set => Memory.WriteMemory(Address + 0x60, value);
        }
        public D3DMatrix RelMatrix
        {
            get => Memory.ReadMemory<D3DMatrix>(Memory.ReadMemory<int>(Address + 0x64));
            set => Memory.WriteMemory(Memory.ReadMemory<int>(Address + 0x64), value);
        } 
        public D3DVector Used_RelVec
        {
            get => Memory.ReadMemory<D3DVector>(Address + 0x68);
            set => Memory.WriteMemory(Address + 0x68, value);
        }
        public int Kachel
        {
            get => Memory.ReadMemory<int>(Address + 0x74); 
            set => Memory.WriteMemory(Address + 0x74, value);
        }
        public D3DMatrix AbsPosition
        {
            get => Memory.ReadMemory<D3DMatrix>(Address + 0x78);
            set => Memory.WriteMemory(Address + 0x78, value);
        }
        public D3DMatrix AbsPosition_Inv
        {
            get => Memory.ReadMemory<D3DMatrix>(Address + 0xb8);
            set => Memory.WriteMemory(Address + 0xb8, value);
        }
        public D3DMatrix AbsPosition_ThreadFree
        {
            get => Memory.ReadMemory<D3DMatrix>(Address + 0xf8);
            set => Memory.WriteMemory(Address + 0xf8, value);
        }
    }
}