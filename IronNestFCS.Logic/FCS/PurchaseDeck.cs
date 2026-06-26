using Il2Cpp;
using MelonLoader;
using UnityEngine;
using System.Collections;

namespace IronNestFCS.Logic.FCS;

public class PurchaseDeck {
    private Transform? _heCard;
    private Transform? _apCard;
    private Transform? _starCard;
    private Transform? _smkCard;
    private Transform? _hcheCard;
    private Transform? _powderCard;
    private LookAtTarget? _buyButton;
    
    
    public bool TryBind() {
        var requisitionConsole = GameObject.Find("Requisition Console").transform;
        var cards = requisitionConsole.GetComponentsInChildren<PunchcardRuntime>();
        foreach (var card in cards) {
            MelonLogger.Msg($"[FCS] PurchaseDeck: Found card {card.CurrentDefinition.ID}");
            switch (card.CurrentDefinition.ID) {
                case "HEShell":
                    _heCard = card.transform;
                    break;
                case "APShell":
                    _apCard = card.transform;
                    break;
                case "STARShell":
                    _starCard = card.transform;
                    break;
                case "SMOKEShell":
                    _smkCard = card.transform;
                    break;
                case "HCHEShell":
                    _hcheCard = card.transform;
                    break;
                case "PowderCharges":
                    _powderCard = card.transform;
                    break;
                default:
                    break;
            }
        }
        
        _buyButton = requisitionConsole.FindChild("Universal Button").GetComponent<LookAtTarget>();
        
        return true;
    }
    
    private DialInteractable GetLeftRightDial() {
        var consoleBox = GameObject.Find("Console Box").transform;
        return  consoleBox.GetComponentInChildren<DialInteractable>();
    }

    public IEnumerator BuyShell(BulletType type, LeftRight leftRight) {
        var card = type switch {
            BulletType.AP => _apCard,
            BulletType.HE => _heCard,
            BulletType.STAR => _starCard,
            BulletType.SMK => _smkCard,
            BulletType.HCHE => _hcheCard,
            _ => null
        };
        if (card == null) {
            MelonLogger.Error($"[FCS] BuyShell: Can't find {type} card");
            yield break;
        }
        var target = new Vector3(6.4814f, -2.4675f, -22.0968f);
        card.position = target;
        card.GetComponent<DraggableItem>().MoveToSlot();
        yield return new WaitForSeconds(0.5f);
        
        switch (leftRight) {
            case LeftRight.Left:
                GetLeftRightDial().SetDialValue(0);
                break;
            case LeftRight.Right:
                GetLeftRightDial().SetDialValue(1);
                break;
        }
        yield return FcsSceneInteractor.WaitAndClick(_buyButton);
        yield return new WaitForSeconds(2f);
    }

    public IEnumerator BuyPowders() {
        if (_powderCard == null) {
            MelonLogger.Error("[FCS] BuyPowders: Can't find PowderCharges card");
            yield break;
        }
        _powderCard.position = new Vector3(6.4814f, -2.4675f, -22.0968f);
        _powderCard.GetComponent<DraggableItem>().MoveToSlot();
        // 与 BuyShell 一致：等卡牌入槽稳定后再点购买，避免点击早于入槽导致本次采购无效。
        yield return new WaitForSeconds(0.5f);
        yield return FcsSceneInteractor.WaitAndClick(_buyButton);
        yield return new WaitForSeconds(2f);
    }
    
}