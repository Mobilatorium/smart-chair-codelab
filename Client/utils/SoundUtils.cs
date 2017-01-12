using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CleverChair.utils
{
    public sealed class SoundUtils
    {
        private const int MMSYSERR_NOERROR = 0;
        private const int MAXPNAMELEN = 32;
        private const int MIXER_LONG_NAME_CHARS = 64;
        private const int MIXER_SHORT_NAME_CHARS = 16;
        private const int MIXER_GETLINEINFOF_COMPONENTTYPE = 0x3;
        private const int MIXER_GETCONTROLDETAILSF_VALUE = 0x0;
        private const int MIXER_GETLINECONTROLSF_ONEBYTYPE = 0x2;
        private const int MIXER_SETCONTROLDETAILSF_VALUE = 0x0;
        private const int MIXERLINE_COMPONENTTYPE_DST_FIRST = 0x0;
        private const int MIXERLINE_COMPONENTTYPE_SRC_FIRST = 0x1000;
        private const int MIXERLINE_COMPONENTTYPE_DST_SPEAKERS = (MIXERLINE_COMPONENTTYPE_DST_FIRST + 4);
        private const int MIXERLINE_COMPONENTTYPE_SRC_MICROPHONE = (MIXERLINE_COMPONENTTYPE_SRC_FIRST + 3);
        private const int MIXERLINE_COMPONENTTYPE_SRC_LINE = (MIXERLINE_COMPONENTTYPE_SRC_FIRST + 2);
        private const int MIXERCONTROL_CT_CLASS_FADER = 0x50000000;
        private const int MIXERCONTROL_CT_UNITS_UNSIGNED = 0x30000;
        private const int MIXERCONTROL_CT_CLASS_SWITCH = 0x20000000;
        private const int MIXERCONTROL_CT_SC_SWITCH_BOOLEAN = 0x0;
        private const int MIXERCONTROL_CT_UNITS_BOOLEAN = 0x10000;
        private const int MIXERCONTROL_CONTROLTYPE_FADER = (MIXERCONTROL_CT_CLASS_FADER | MIXERCONTROL_CT_UNITS_UNSIGNED);
        private const int MIXERCONTROL_CONTROLTYPE_VOLUME = (MIXERCONTROL_CONTROLTYPE_FADER + 1);
        private const int MIXERCONTROL_CONTROLTYPE_BOOLEAN = (MIXERCONTROL_CT_CLASS_SWITCH | MIXERCONTROL_CT_SC_SWITCH_BOOLEAN | MIXERCONTROL_CT_UNITS_BOOLEAN);

        private const int MIXERCONTROL_CONTROLTYPE_MUTE = (MIXERCONTROL_CONTROLTYPE_BOOLEAN + 2);

        private struct MIXERCAPS
        {
            public int wMid;
            public int wPid;
            public int vDriverVersion;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAXPNAMELEN)]
            public string szPname;
            public int fdwSupport;
            public int cDestinations;
        }

        private struct MIXERCONTROL
        {
            public int cbStruct;
            public int dwControlID;
            public int dwControlType;
            public int fdwControl;
            public int cMultipleItems;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MIXER_SHORT_NAME_CHARS)]
            public string szShortName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MIXER_LONG_NAME_CHARS)]
            public string szName;
            public int lMinimum;
            public int lMaximum;
            [MarshalAs(UnmanagedType.U4, SizeConst = 10)]
            public int reserved;
        }

        private struct MIXERCONTROLDETAILS
        {
            public int cbStruct;
            public int dwControlID;
            public int cChannels;
            public int item;
            public int cbDetails;
            public IntPtr paDetails;
        }

        private struct MIXERCONTROLDETAILS_UNSIGNED
        {
            public int dwValue;
        }

        private struct MIXERCONTROLDETAILS_BOOLEAN
        {
            public int dwValue;
        }

        private struct MIXERLINE
        {
            public int cbStruct;
            public int dwDestination;
            public int dwSource;
            public int dwLineID;
            public int fdwLine;
            public int dwUser;
            public int dwComponentType;
            public int cChannels;
            public int cConnections;
            public int cControls;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MIXER_SHORT_NAME_CHARS)]
            public string szShortName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MIXER_LONG_NAME_CHARS)]
            public string szName;
            public int dwType;
            public int dwDeviceID;
            public int wMid;
            public int wPid;
            public int vDriverVersion;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAXPNAMELEN)]
            public string szPname;
        }

        private struct MIXERLINECONTROLS
        {
            public int cbStruct;
            public int dwLineID;

            public int dwControl;
            public int cControls;
            public int cbmxctrl;
            public IntPtr pamxctrl;
        }

        [DllImport("winmm.dll", CharSet = CharSet.Ansi)]
        private static extern int mixerClose(int hmx);
        [DllImport("winmm.dll", CharSet = CharSet.Ansi)]
        private static extern int mixerGetControlDetailsA(int hmxobj, ref MIXERCONTROLDETAILS pmxcd, int fdwDetails);
        [DllImport("winmm.dll", CharSet = CharSet.Ansi)]
        private static extern int mixerGetDevCapsA(int uMxId, MIXERCAPS pmxcaps, int cbmxcaps);
        [DllImport("winmm.dll", CharSet = CharSet.Ansi)]
        private static extern int mixerGetID(int hmxobj, int pumxID, int fdwId);
        [DllImport("winmm.dll", CharSet = CharSet.Ansi)]
        private static extern int mixerGetLineControlsA(int hmxobj, ref MIXERLINECONTROLS pmxlc, int fdwControls);
        [DllImport("winmm.dll", CharSet = CharSet.Ansi)]
        private static extern int mixerGetLineInfoA(int hmxobj, ref MIXERLINE pmxl, int fdwInfo);
        [DllImport("winmm.dll", CharSet = CharSet.Ansi)]
        private static extern int mixerGetNumDevs();
        [DllImport("winmm.dll", CharSet = CharSet.Ansi)]
        private static extern int mixerMessage(int hmx, int uMsg, int dwParam1, int dwParam2);
        [DllImport("winmm.dll", CharSet = CharSet.Ansi)]
        private static extern int mixerOpen(out int phmx, int uMxId, int dwCallback, int dwInstance, int fdwOpen);
        [DllImport("winmm.dll", CharSet = CharSet.Ansi)]
        private static extern int mixerSetControlDetails(int hmxobj, ref MIXERCONTROLDETAILS pmxcd, int fdwDetails);

        [DllImport("winmm.dll", EntryPoint = "sndPlaySoundA")]
        private static extern int sndPlaySound(string lpszSoundName, int uFlags);
        private const int SND_ASYNC = 0x1;
        private const int SND_LOOP = 0x8;

        public static void PlaySound(string filename, bool loop)
        {
            if (File.Exists(filename))
            {
                if (loop)
                    sndPlaySound(filename, SND_ASYNC | SND_LOOP);
                else
                    sndPlaySound(filename, SND_ASYNC);
            }
        }

        public static void StopSound()
        {
            sndPlaySound(null, SND_ASYNC);
        }

        public static bool isMuted(bool isOut)
        {
            EndpointVolume epVol = new EndpointVolume(isOut);
            return epVol.Mute;
        }

        public static void Mute(bool isOut, bool mute)
        {
            // Check if this is vista or XP
            if (System.Environment.OSVersion.Version.Major >= 6)
            {
                EndpointVolume epVol = new EndpointVolume(isOut);
                epVol.Mute = mute;
                epVol.Dispose();
            }
            else
            {
                int mixerID = 0;

                if (mixerOpen(out mixerID, 0, 0, 0, 0) == MMSYSERR_NOERROR)
                {
                    MIXERLINE line = new MIXERLINE();
                    line.cbStruct = Marshal.SizeOf(line);
                    line.dwComponentType = MIXERLINE_COMPONENTTYPE_DST_SPEAKERS;

                    if (mixerGetLineInfoA(mixerID, ref line, MIXER_GETLINEINFOF_COMPONENTTYPE) == MMSYSERR_NOERROR)
                    {
                        int sizeofMIXERCONTROL = 152;
                        MIXERCONTROL mixerControl = new MIXERCONTROL();

                        MIXERLINECONTROLS lineControl = new MIXERLINECONTROLS();

                        lineControl.pamxctrl = Marshal.AllocCoTaskMem(sizeofMIXERCONTROL);
                        lineControl.cbStruct = Marshal.SizeOf(lineControl);
                        lineControl.dwLineID = line.dwLineID;
                        lineControl.dwControl = MIXERCONTROL_CONTROLTYPE_MUTE;
                        lineControl.cControls = 1;
                        lineControl.cbmxctrl = sizeofMIXERCONTROL;

                        mixerControl.cbStruct = sizeofMIXERCONTROL;

                        if (mixerGetLineControlsA(mixerID, ref lineControl, MIXER_GETLINECONTROLSF_ONEBYTYPE) == MMSYSERR_NOERROR)
                        {
                            mixerControl = (MIXERCONTROL)Marshal.PtrToStructure(lineControl.pamxctrl, typeof(MIXERCONTROL));

                            MIXERCONTROLDETAILS controlDetails = new MIXERCONTROLDETAILS();
                            MIXERCONTROLDETAILS_BOOLEAN muteValue = new MIXERCONTROLDETAILS_BOOLEAN();

                            controlDetails.item = 0;
                            controlDetails.dwControlID = mixerControl.dwControlID;
                            controlDetails.cbStruct = Marshal.SizeOf(controlDetails);
                            controlDetails.cbDetails = Marshal.SizeOf(muteValue);
                            controlDetails.cChannels = 1;

                            muteValue.dwValue = Convert.ToInt32(mute);

                            controlDetails.paDetails = Marshal.AllocCoTaskMem(Marshal.SizeOf(muteValue));

                            Marshal.StructureToPtr(muteValue, controlDetails.paDetails, false);

                            int rc = mixerSetControlDetails(mixerID, ref controlDetails, MIXER_SETCONTROLDETAILSF_VALUE);

                            Marshal.FreeCoTaskMem(controlDetails.paDetails);
                            Marshal.FreeCoTaskMem(lineControl.pamxctrl);
                        }
                    }
                }
            }
        }
    }

    public class EndpointVolume
    {

        #region Interface to COM objects

        const int DEVICE_STATE_ACTIVE = 0x00000001;

        const int DEVICE_STATE_DISABLE = 0x00000002;

        const int DEVICE_STATE_NOTPRESENT = 0x00000004;

        const int DEVICE_STATE_UNPLUGGED = 0x00000008;

        const int DEVICE_STATEMASK_ALL = 0x0000000f;

        [DllImport("ole32.Dll")]

        static public extern uint CoCreateInstance(ref Guid clsid,

        [MarshalAs(UnmanagedType.IUnknown)] object inner,

        uint context,

        ref Guid uuid,

        [MarshalAs(UnmanagedType.IUnknown)] out object rReturnedComObject);



        // C Header file : Include Mmdeviceapi.h (Windows Vista SDK)

        [Guid("5CDF2C82-841E-4546-9722-0CF74078229A"),

        InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]

        public interface IAudioEndpointVolume
        {

            //virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE RegisterControlChangeNotify(/* [in] */__in IAudioEndpointVolumeCallback *pNotify) = 0;

            //int RegisterControlChangeNotify(IntPtr pNotify);

            int RegisterControlChangeNotify(DelegateMixerChange pNotify);

            //virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE UnregisterControlChangeNotify(/* [in] */ __in IAudioEndpointVolumeCallback *pNotify) = 0;

            //int UnregisterControlChangeNotify(IntPtr pNotify);

            int UnregisterControlChangeNotify(DelegateMixerChange pNotify);

            //virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE GetChannelCount(/* [out] */ __out UINT *pnChannelCount) = 0;

            int GetChannelCount(ref uint pnChannelCount);

            //virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE SetMasterVolumeLevel( /* [in] */ __in float fLevelDB,/* [unique][in] */ LPCGUID pguidEventContext) = 0;

            int SetMasterVolumeLevel(float fLevelDB, Guid pguidEventContext);

            //virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE SetMasterVolumeLevelScalar( /* [in] */ __in float fLevel,/* [unique][in] */ LPCGUID pguidEventContext) = 0;

            int SetMasterVolumeLevelScalar(float fLevel, Guid pguidEventContext);

            //virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE GetMasterVolumeLevel(/* [out] */ __out float *pfLevelDB) = 0;

            int GetMasterVolumeLevel(ref float pfLevelDB);

            //virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE GetMasterVolumeLevelScalar( /* [out] */ __out float *pfLevel) = 0;

            int GetMasterVolumeLevelScalar(ref float pfLevel);

            //virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE SetChannelVolumeLevel(/* [in] */__in UINT nChannel,float fLevelDB,/* [unique][in] */ LPCGUID pguidEventContext) = 0;

            int SetChannelVolumeLevel(uint nChannel, float fLevelDB, Guid pguidEventContext);

            //virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE SetChannelVolumeLevelScalar(/* [in] */ __in UINT nChannel,float fLevel,/* [unique][in] */ LPCGUID pguidEventContext) = 0;

            int SetChannelVolumeLevelScalar(uint nChannel, float fLevel, Guid pguidEventContext);

            //virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE GetChannelVolumeLevel(/* [in] */ __in UINT nChannel,/* [out] */__out float *pfLevelDB) = 0;

            int GetChannelVolumeLevel(uint nChannel, ref float pfLevelDB);

            //virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE GetChannelVolumeLevelScalar(/* [in] */__in UINT nChannel,/* [out] */__out float *pfLevel) = 0;

            int GetChannelVolumeLevelScalar(uint nChannel, ref float pfLevel);

            //virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE SetMute( /* [in] *__in BOOL bMute, /* [unique][in] */ LPCGUID pguidEventContext) = 0;

            int SetMute(int bMute, Guid pguidEventContext);

            //virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE GetMute( /* [out] */ __out BOOL *pbMute) = 0;

            int GetMute(ref bool pbMute);

            //virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE GetVolumeStepInfo( /* [out] */ __out UINT *pnStep,/* [out] */__out UINT *pnStepCount) = 0;

            int GetVolumeStepInfo(ref uint pnStep, ref uint pnStepCount);

            //virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE VolumeStepUp( /* [unique][in] */ LPCGUID pguidEventContext) = 0;

            int VolumeStepUp(Guid pguidEventContext);

            //virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE VolumeStepDown(/* [unique][in] */ LPCGUID pguidEventContext) = 0;

            int VolumeStepDown(Guid pguidEventContext);

            //virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE QueryHardwareSupport(/* [out] */ __out DWORD *pdwHardwareSupportMask) = 0;

            int QueryHardwareSupport(ref uint pdwHardwareSupportMask);

            //virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE GetVolumeRange( /* [out] */ __out float *pflVolumeMindB,/* [out] */ __out float *pflVolumeMaxdB,/* [out] */ __out float *pflVolumeIncrementdB) = 0;

            int GetVolumeRange(ref float pflVolumeMindB, ref float pflVolumeMaxdB, ref float pflVolumeIncrementdB);

        }

        [Guid("0BD7A1BE-7A1A-44DB-8397-CC5392387B5E"),

        InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]

        public interface IMMDeviceCollection
        {

            //HRESULT GetCount([out, annotation("__out")] UINT* pcDevices);

            int GetCount(ref uint pcDevices);

            //HRESULT Item([in, annotation("__in")]UINT nDevice, [out, annotation("__out")] IMMDevice** ppDevice);

            int Item(uint nDevice, ref IntPtr ppDevice);

        }

        [Guid("D666063F-1587-4E43-81F1-B948E807363F"),

        InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]

        public interface IMMDevice
        {

            //HRESULT Activate([in, annotation("__in")] REFIID iid, [in, annotation("__in")] DWORD dwClsCtx, [in,unique, annotation("__in_opt")] PROPVARIANT* pActivationParams, [out,iid_is(iid), annotation("__out")] void** ppInterface);

            int Activate(ref Guid iid, uint dwClsCtx, IntPtr pActivationParams, ref IntPtr ppInterface);

            //HRESULT OpenPropertyStore([in, annotation("__in")] DWORD stgmAccess, [out, annotation("__out")] IPropertyStore** ppProperties);

            int OpenPropertyStore(int stgmAccess, ref IntPtr ppProperties);

            //HRESULT GetId([out,annotation("__deref_out")] LPWSTR* ppstrId);

            int GetId(ref string ppstrId);

            //HRESULT GetState([out, annotation("__out")] DWORD* pdwState);

            int GetState(ref int pdwState);

        }

        [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"),

        //[Guid("BCDE0395-E52F-467C-8E3D-C4579291692E"),

        InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]

        public interface IMMDeviceEnumerator
        {

            //HRESULT EnumAudioEndpoints([in, annotation("__in")] EDataFlow dataFlow, [in, annotation("__in")] DWORD dwStateMask, [out, annotation("__out")] IMMDeviceCollection** ppDevices);

            int EnumAudioEndpoints(EDataFlow dataFlow, int dwStateMask, ref IntPtr ppDevices);

            //HRESULT GetDefaultAudioEndpoint([in, annotation("__in")] EDataFlow dataFlow, [in, annotation("__in")] ERole role, [out, annotation("__out")] IMMDevice** ppEndpoint);

            int GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role, ref IntPtr ppEndpoint);

            //HRESULT GetDevice([, annotation("__in")]LPCWSTR pwstrId, [out, annotation("__out")] IMMDevice** ppDevice);

            int GetDevice(string pwstrId, ref IntPtr ppDevice);

            //HRESULT RegisterEndpointNotificationCallback([in, annotation("__in")] IMMNotificationClient* pClient);

            int RegisterEndpointNotificationCallback(IntPtr pClient);

            //HRESULT UnregisterEndpointNotificationCallback([in, annotation("__in")] IMMNotificationClient* pClient);

            int UnregisterEndpointNotificationCallback(IntPtr pClient);

        }

        [Flags]

        enum CLSCTX : uint
        {

            CLSCTX_INPROC_SERVER = 0x1,

            CLSCTX_INPROC_HANDLER = 0x2,

            CLSCTX_LOCAL_SERVER = 0x4,

            CLSCTX_INPROC_SERVER16 = 0x8,

            CLSCTX_REMOTE_SERVER = 0x10,

            CLSCTX_INPROC_HANDLER16 = 0x20,

            CLSCTX_RESERVED1 = 0x40,

            CLSCTX_RESERVED2 = 0x80,

            CLSCTX_RESERVED3 = 0x100,

            CLSCTX_RESERVED4 = 0x200,

            CLSCTX_NO_CODE_DOWNLOAD = 0x400,

            CLSCTX_RESERVED5 = 0x800,

            CLSCTX_NO_CUSTOM_MARSHAL = 0x1000,

            CLSCTX_ENABLE_CODE_DOWNLOAD = 0x2000,

            CLSCTX_NO_FAILURE_LOG = 0x4000,

            CLSCTX_DISABLE_AAA = 0x8000,

            CLSCTX_ENABLE_AAA = 0x10000,

            CLSCTX_FROM_DEFAULT_CONTEXT = 0x20000,

            CLSCTX_INPROC = CLSCTX_INPROC_SERVER | CLSCTX_INPROC_HANDLER,

            CLSCTX_SERVER = CLSCTX_INPROC_SERVER | CLSCTX_LOCAL_SERVER | CLSCTX_REMOTE_SERVER,

            CLSCTX_ALL = CLSCTX_SERVER | CLSCTX_INPROC_HANDLER

        }

        public enum EDataFlow
        {

            eRender,

            eCapture,

            eAll,

            EDataFlow_enum_count

        }

        public enum ERole
        {

            eConsole,

            eMultimedia,

            eCommunications,

            ERole_enum_count

        }

        #endregion



        // Private internal var

        object oEnumerator = null;

        IMMDeviceEnumerator iMde = null;

        object oDevice = null;

        IMMDevice imd = null;

        object oEndPoint = null;

        IAudioEndpointVolume iAudioEndpoint = null;

        // TODO

        // Problem #1 : I can't handle a volume changed event by other applications

        // (example while using the program SndVol.exe)

        public delegate void DelegateMixerChange();

        //public DelegateMixerChange delMixerChange = null;

        public delegate void MixerChangedEventHandler();

        //public event MixerChangedEventHandler MixerChanged;



        #region Class Constructor and Dispose public methods

        /// <summary>

        /// Constructor

        /// </summary>

        public EndpointVolume(bool isOut)
        {

            const uint CLSCTX_INPROC_SERVER = 1;

            Guid clsid = new Guid("BCDE0395-E52F-467C-8E3D-C4579291692E");

            Guid IID_IUnknown = new Guid("00000000-0000-0000-C000-000000000046");

            oEnumerator = null;

            uint hResult = CoCreateInstance(ref clsid, null, CLSCTX_INPROC_SERVER, ref IID_IUnknown, out oEnumerator);

            if (hResult != 0 || oEnumerator == null)
            {

                throw new Exception("CoCreateInstance() pInvoke failed");

            }

            iMde = oEnumerator as IMMDeviceEnumerator;

            if (iMde == null)
            {

                throw new Exception("COM cast failed to IMMDeviceEnumerator");

            }

            IntPtr pDevice = IntPtr.Zero;

            int retVal = iMde.GetDefaultAudioEndpoint(isOut ? EDataFlow.eRender : EDataFlow.eCapture, ERole.eConsole, ref pDevice);

            if (retVal != 0)
            {

                throw new Exception("IMMDeviceEnumerator.GetDefaultAudioEndpoint()");

            }

            int dwStateMask = DEVICE_STATE_ACTIVE | DEVICE_STATE_NOTPRESENT | DEVICE_STATE_UNPLUGGED;

            IntPtr pCollection = IntPtr.Zero;

            retVal = iMde.EnumAudioEndpoints(EDataFlow.eRender, dwStateMask, ref pCollection);

            if (retVal != 0)
            {

                throw new Exception("IMMDeviceEnumerator.EnumAudioEndpoints()");

            }

            oDevice = System.Runtime.InteropServices.Marshal.GetObjectForIUnknown(pDevice);

            imd = oDevice as IMMDevice;

            if (imd == null)
            {

                throw new Exception("COM cast failed to IMMDevice");

            }

            Guid iid = new Guid("5CDF2C82-841E-4546-9722-0CF74078229A");

            uint dwClsCtx = (uint)CLSCTX.CLSCTX_ALL;

            IntPtr pActivationParams = IntPtr.Zero;

            IntPtr pEndPoint = IntPtr.Zero;

            retVal = imd.Activate(ref iid, dwClsCtx, pActivationParams, ref pEndPoint);

            if (retVal != 0)
            {

                throw new Exception("IMMDevice.Activate()");

            }

            oEndPoint = System.Runtime.InteropServices.Marshal.GetObjectForIUnknown(pEndPoint);

            iAudioEndpoint = oEndPoint as IAudioEndpointVolume;

            if (iAudioEndpoint == null)
            {

                throw new Exception("COM cast failed to IAudioEndpointVolume");

            }

            /*

            delMixerChange = new DelegateMixerChange(MixerChange);

            retVal = iAudioEndpoint.RegisterControlChangeNotify(delMixerChange);

            if (retVal != 0)

            {

            throw new Exception("iAudioEndpoint.RegisterControlChangeNotify(delMixerChange)");

            }

            */

        }

        /// <summary>

        /// Call this method to release all com objetcs

        /// </summary>

        public virtual void Dispose()
        {

            /*

            if (delMixerChange != null && iAudioEndpoint != null)

            {

            iAudioEndpoint.UnregisterControlChangeNotify(delMixerChange);

            }

            */

            if (iAudioEndpoint != null)
            {

                System.Runtime.InteropServices.Marshal.ReleaseComObject(iAudioEndpoint);

                iAudioEndpoint = null;

            }

            if (oEndPoint != null)
            {

                System.Runtime.InteropServices.Marshal.ReleaseComObject(oEndPoint);

                oEndPoint = null;

            }

            if (imd != null)
            {

                System.Runtime.InteropServices.Marshal.ReleaseComObject(imd);

                imd = null;

            }

            if (oDevice != null)
            {

                System.Runtime.InteropServices.Marshal.ReleaseComObject(oDevice);

                oDevice = null;

            }

            //System.Runtime.InteropServices.Marshal.ReleaseComObject(pCollection);

            if (iMde != null)
            {

                System.Runtime.InteropServices.Marshal.ReleaseComObject(iMde);

                iMde = null;

            }

            if (oEnumerator != null)
            {

                System.Runtime.InteropServices.Marshal.ReleaseComObject(oEnumerator);

                oEnumerator = null;

            }

        }

        #endregion

        #region Private internal functions


        private void MixerChange()
        {

            /*

            if (MixerChanged != null)

            // Notify (raise) event

            MixerChanged();

            */

        }

        #endregion

        #region Public properties

        /// <summary>

        /// Get/set the master mute. WARNING : The set mute do NOT work!

        /// </summary>

        public bool Mute
        {

            get
            {

                bool mute = false;

                int retVal = iAudioEndpoint.GetMute(ref mute);

                if (retVal != 0)
                {

                    throw new Exception("IAudioEndpointVolume.GetMute() failed!");

                }

                return mute;

            }

            set
            {

                //nullGuid = new Guid("{00000000-0000-0000-0000-000000000000}"); // null

                // {dddddddd-dddd-dddd-dddd-dddddddddddd}

                Guid nullGuid = Guid.Empty;

                bool mute = value;

                // TODO

                // Problem #2 : This function always terminate with an internal error!

                int retVal = iAudioEndpoint.SetMute(Convert.ToInt32(mute), nullGuid);

                if (retVal != 0)
                {

                    throw new Exception("IAudioEndpointVolume.SetMute() failed!");

                }

            }

        }

        /// <summary>

        /// Get/set the master volume level. Valid range is from 0.00F (0%) to 1.00F (100%).

        /// </summary>

        public float MasterVolume
        {

            get
            {

                float level = 0.0F;

                int retVal = iAudioEndpoint.GetMasterVolumeLevelScalar(ref level);

                if (retVal != 0)
                {

                    throw new Exception("IAudioEndpointVolume.GetMasterVolumeLevelScalar()");

                }

                return level;

            }

            set
            {

                float level = value;

                Guid nullGuid;

                //nullGuid = new Guid("{00000000-0000-0000-0000-000000000000}"); // null

                // {dddddddd-dddd-dddd-dddd-dddddddddddd}

                nullGuid = Guid.Empty;

                int retVal = iAudioEndpoint.SetMasterVolumeLevelScalar(level, nullGuid);

                if (retVal != 0)
                {

                    throw new Exception("IAudioEndpointVolume.SetMasterVolumeLevelScalar()");

                }

            }

        }

        #endregion

        #region Public Methods

        /// <summary>

        /// Increase the master volume

        /// </summary>

        public void VolumeUp()
        {

            Guid nullGuid;

            //nullGuid = new Guid("{00000000-0000-0000-0000-000000000000}"); // null

            // {dddddddd-dddd-dddd-dddd-dddddddddddd}

            nullGuid = Guid.Empty;

            int retVal = iAudioEndpoint.VolumeStepUp(nullGuid);

            if (retVal != 0)
            {

                throw new Exception("IAudioEndpointVolume.SetMute()");

            }

        }

        /// <summary>

        /// Decrease the master volume

        /// </summary>

        public void VolumeDown()
        {

            Guid nullGuid;

            //nullGuid = new Guid("{00000000-0000-0000-0000-000000000000}"); // null

            // {dddddddd-dddd-dddd-dddd-dddddddddddd}

            nullGuid = Guid.Empty;

            int retVal = iAudioEndpoint.VolumeStepDown(nullGuid);

        }

        #endregion
    }
}
