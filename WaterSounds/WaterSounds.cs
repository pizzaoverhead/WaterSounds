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
        public AudioSource waterSource;
        private bool _inWater = false;
        private bool _paused = false;
        private bool _enabled = true;

        public void Start()
        {
            try
            {
                waterSource = gameObject.AddComponent<AudioSource>();
                waterSource.clip = GameDatabase.Instance.GetAudioClip(waterSoundFile);
                if (waterSource.clip == null)
                {
                    _enabled = false;
                    return;
                }
                waterSource.loop = true;
                waterSource.volume = GameSettings.AMBIENCE_VOLUME;
                waterSource.dopplerLevel = 0f;
                waterSource.rolloffMode = AudioRolloffMode.Logarithmic;
                waterSource.minDistance = 0.5f;
                waterSource.maxDistance = 1f;

                GameEvents.onGamePause.Add(Pause);
                GameEvents.onGameUnpause.Add(Unpause);
                GameEvents.onVesselSituationChange.Add(VesselSituationChange);
                GameEvents.onVesselChange.Add(VesselChanged);
            }
            catch (Exception ex)
            {
                Debug.LogError("[WaterSounds] Startup error: " + ex.Message + "\n");
                _enabled = false;
            }
        }

        public void Pause()
        {
            _paused = true;
            if (waterSource != null && waterSource.isPlaying)
                waterSource.Stop();
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
            if (waterSource != null)
            waterSource.Stop();
            GameEvents.onGamePause.Remove(Pause);
            GameEvents.onGameUnpause.Remove(Unpause);
            GameEvents.onVesselSituationChange.Remove(VesselSituationChange);
            GameEvents.onVesselLoaded.Remove(VesselChanged);
        }

        public void Update()
        {
            try
            {
                if (waterSource == null)
                    _enabled = false;
                if (!_enabled)
                    return;
                if (_inWater && !_paused && FlightGlobals.ActiveVessel != null &&
                    FlightGlobals.ActiveVessel.srf_velocity.magnitude < 10)
                {
                    gameObject.transform.position = FlightGlobals.ActiveVessel.mainBody.GetWorldSurfacePosition(
                        FlightGlobals.ActiveVessel.latitude, FlightGlobals.ActiveVessel.longitude, 0);

                    if (!waterSource.isPlaying)
                        waterSource.Play();
                }
                else
                {
                    /*string err = "";
                    if (FlightGlobals.ActiveVessel == null) err = "Active vessel null";
                    else if (_paused) err = "Paused";
                    else if (FlightGlobals.ActiveVessel.srf_velocity.magnitude < 10) err = "Too fast: " + FlightGlobals.ActiveVessel.srf_velocity.magnitude;
                    else if (!_inWater) err = "Not in water";
                    Debug.Log("Water sounds stopping: " + err);*/
                    waterSource.Stop();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[WaterSounds] Update error: " + ex.Message + "\n");
                _enabled = false;
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