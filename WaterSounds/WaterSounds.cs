using UnityEngine;
using System;

namespace WaterSounds
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    class WaterSounds : MonoBehaviour
    {
        // TODO:
        // Add sounds to objects floating in water.
        // Add sounds when objects are moving in water.
        // Add sound when leaving water.

        [KSPField]
        public string waterSoundFile = "WaterSounds/Sounds/WaterIdle";
        [KSPField]
        public string underwaterSoundFile = "WaterSounds/Sounds/UnderwaterAmbiance";
        [KSPField]
        public string diveSoundFile = "WaterSounds/Sounds/Submerge";
        [KSPField]
        public string surfaceSoundFile = "WaterSounds/Sounds/Emerge";
        [KSPField]
        public float pitchRange = 0.3f;
        public AudioSource waterSound;
        public AudioSource underwaterSound;
        public AudioSource diveSound;
        public AudioSource surfaceSound;
        private bool _inWater = false;
        private bool _wasUnderWater = false;
        private bool _paused = false;
        private bool _validWaterSound = true;
        private bool _validUnderwaterSound = true;
        private bool _validDiveSound = true;
        private bool _validSurfaceSound = true;

        public void Start()
        {
            try
            {
                waterSound = Utils.InitAudioSource(waterSoundFile, gameObject, GameSettings.AMBIENCE_VOLUME, true, true);
                if (waterSound.clip == null)
                    _validWaterSound = false;

                underwaterSound = Utils.InitAudioSource(underwaterSoundFile, gameObject, GameSettings.AMBIENCE_VOLUME, true, true);
                if (underwaterSound.clip == null)
                    _validUnderwaterSound = false;

                diveSound = Utils.InitAudioSource(diveSoundFile, gameObject, GameSettings.SHIP_VOLUME, false, false);
                if (diveSound.clip == null)
                    _validDiveSound = false;

                surfaceSound = Utils.InitAudioSource(surfaceSoundFile, gameObject, GameSettings.SHIP_VOLUME, false, false);
                if (diveSound.clip == null)
                    _validSurfaceSound = false;

                GameEvents.onGamePause.Add(Pause);
                GameEvents.onGameUnpause.Add(Unpause);
                GameEvents.onVesselSituationChange.Add(VesselSituationChange);
                GameEvents.onVesselChange.Add(VesselChanged);
            }
            catch (Exception ex)
            {
                Debug.LogError("[WaterSounds] Startup error: " + ex.Message + "\n");
                _validWaterSound = false;
            }
        }

        public void Pause()
        {
            _paused = true;
            if (waterSound != null && waterSound.isPlaying)
                waterSound.Pause();
        }

        public void Unpause()
        {
            _paused = false;
        }

        public void VesselSituationChange(GameEvents.HostedFromToAction<Vessel, Vessel.Situations> a)
        {
            _inWater = FlightGlobals.ActiveVessel.situation == Vessel.Situations.SPLASHED;
        }

        public void VesselChanged(Vessel v)
        {
            _inWater = FlightGlobals.ActiveVessel.situation == Vessel.Situations.SPLASHED;
        }

        public void Destroy()
        {
            if (waterSound != null)
            waterSound.Stop();
            GameEvents.onGamePause.Remove(Pause);
            GameEvents.onGameUnpause.Remove(Unpause);
            GameEvents.onVesselSituationChange.Remove(VesselSituationChange);
            GameEvents.onVesselLoaded.Remove(VesselChanged);
        }

        public void Update()
        {
            try
            {
                FlightCamera flightCamera = FlightCamera.fetch;
                bool underwater = flightCamera.cameraAlt < 0;
                if (underwaterSound == null)
                    _validUnderwaterSound = false;
                if (waterSound == null)
                    _validWaterSound = false;

                if (underwater)
                {
                    if (!_wasUnderWater && _validDiveSound)
                    {
                        diveSound.pitch = UnityEngine.Random.Range(1 - pitchRange, 1 + pitchRange);
                        diveSound.Play();
                        _wasUnderWater = true;
                    }
                }
                else if (_wasUnderWater && _validSurfaceSound)
                {
                    surfaceSound.pitch = UnityEngine.Random.Range(1 - pitchRange, 1 + pitchRange);
                    surfaceSound.Play();
                    _wasUnderWater = false;
                }

                if (underwater)
                {
                    if (_validWaterSound)
                        waterSound.Pause();

                    if (!_validUnderwaterSound)
                        return;

                    if (!underwaterSound.isPlaying)
                        underwaterSound.Play();
                }
                else
                {
                    if (_validUnderwaterSound)
                        underwaterSound.Pause();

                    if (!_validWaterSound)
                        return;

                    if (_inWater && !_paused && FlightGlobals.ActiveVessel != null &&
                        FlightGlobals.ActiveVessel.srf_velocity.magnitude < 10)
                    {
                        gameObject.transform.position = FlightGlobals.ActiveVessel.mainBody.GetWorldSurfacePosition(
                            FlightGlobals.ActiveVessel.latitude, FlightGlobals.ActiveVessel.longitude, 0);

                        if (!waterSound.isPlaying)
                            waterSound.Play();
                    }
                    else
                    {
                        /*string err = "";
                        if (FlightGlobals.ActiveVessel == null) err = "Active vessel null";
                        else if (_paused) err = "Paused";
                        else if (FlightGlobals.ActiveVessel.srf_velocity.magnitude < 10) err = "Too fast: " + FlightGlobals.ActiveVessel.srf_velocity.magnitude;
                        else if (!_inWater) err = "Not in water";
                        Debug.Log("Water sounds stopping: " + err);*/
                        waterSound.Pause();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[WaterSounds] Update error: " + ex.Message + "\n");
                _validWaterSound = false;
            }
        }
    }
}

#if DEBUG
[KSPAddon(KSPAddon.Startup.MainMenu, false)]
public class Debug_AutoLoadQuicksaveOnStartup : UnityEngine.MonoBehaviour
{
    public static bool first = true;
    public void Start()
    {
        if (first)
        {
            first = false;
            HighLogic.SaveFolder = "test";
            var game = GamePersistence.LoadGame("quicksave", HighLogic.SaveFolder, true, false);
            if (game != null && game.flightState != null && game.compatible)
            {
                FlightDriver.StartAndFocusVessel(game, game.flightState.activeVesselIdx);
            }
            CheatOptions.InfiniteFuel = true;
            CheatOptions.InfiniteRCS = true;
        }
    }
}
#endif