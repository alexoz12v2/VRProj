using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class FMODEvents : Singleton<FMODEvents>
{
    /*[field: Header("Dissolve SFX")]
    [field: SerializeField] public EventReference dissolve { get; private set; }
    */
    [field: Header("Player footsteps SFX")]
    [field: SerializeField] public EventReference PlayerFootsteps { get; private set; }
    [field: Header("TrinceaSFX")]
    [field: SerializeField] public EventReference Trincea { get; private set; }
    [field: Header("Ambient Sound")]
    [field: SerializeField] public EventReference AmbientSound { get; private set; }
    [field: Header("Main Menu")]
    [field: SerializeField] public EventReference BackgroundUI { get; private set; }
    [field: Header("UI")]
    [field: SerializeField] public EventReference ClickUI { get; private set; }

    [field: Header("Voice/Interactions/FiatRevelli")]
    [field: SerializeField] public EventReference FiatRevelliOnLancia1z { get; private set; }
    [field: SerializeField] public EventReference FiatRevelliOnForest { get; private set; }
    [field: SerializeField] public EventReference FiatRevelliWithSoldiers { get; private set; }
    [field: SerializeField] public EventReference FiatRevelliRiconquistaLibia { get; private set; }

    [field: Header("Voice/Interactions/Carcano")]
    [field: SerializeField] public EventReference CarcanoIntro { get; private set; }
    [field: SerializeField] public EventReference CarcanoVerifica { get; private set; }
    [field: SerializeField] public EventReference CarcanoTecnicismi { get; private set; }
    [field: SerializeField] public EventReference CarcanoDopoguerra { get; private set; }

    [field: Header("Voice/Interactions/Bomba")]
    [field: SerializeField] public EventReference BombaFoto { get; private set; }
    [field: SerializeField] public EventReference BombaProietti { get; private set; }
    [field: SerializeField] public EventReference BombaTrincea { get; private set; }

    [field: Header("Voice/Interactions/Helmet")]
    [field: SerializeField] public EventReference HelmetIntro { get; private set; }
    [field: SerializeField] public EventReference HelmetAlpini { get; private set; }
    [field: SerializeField] public EventReference HelmetPettinengo { get; private set; }

    [field: Header("Voice/Interactions/Pugnale")]
    [field: SerializeField] public EventReference PugnaleAccoppati { get; private set; }
    [field: SerializeField] public EventReference PugnaleArditi { get; private set; }

    [field: Header("Voice/Interactions/Mask")]
    [field: SerializeField] public EventReference MaskChimica { get; private set; }
    [field: SerializeField] public EventReference MaskPolivalente { get; private set; }

    protected override void OnDestroyCallback()
    {
    }
}
