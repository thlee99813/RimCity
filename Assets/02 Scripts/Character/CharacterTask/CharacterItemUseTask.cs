public class CharacterItemUseTask
{
    public void RunEatAction(CharacterEntity owner, int smallTurn, SmallTurnLogController log, float berryHungerRecoverAmount)
    {
        PlayerResourceInventory inv = GameManager.Instance.PlayerInventory;
        if (!inv.Consume(ResourceType.Berry, 1))
        {
            log.AddLog($"[{smallTurn} 턴] {owner.Data.Name}은/는 먹을 산딸기가 없습니다.");
            return;
        }

        owner.Status.AddHunger(berryHungerRecoverAmount, owner.Data);
        log.AddLog($"[{smallTurn} 턴] {owner.Data.Name}은/는 산딸기를 먹습니다.");
    }

    public void RunEquipWoodenSpearTurn(CharacterEntity owner, int smallTurn, SmallTurnLogController log)
    {
        PlayerResourceInventory inv = GameManager.Instance.PlayerInventory;
        if (!inv.ConsumeItem(ItemIds.WoodenSpear, 1))
        {
            log.AddLog($"[{smallTurn} 턴] {owner.Data.Name}은/는 나무창이 없습니다.");
            return;
        }

        owner.EquipWeaponWithSwap(WeaponType.WoodenSpear, inv);
        log.AddLog($"[{smallTurn} 턴] {owner.Data.Name}은/는 나무창을 착용합니다.");
    }

    public void RunEquipStoneSpearTurn(CharacterEntity owner, int smallTurn, SmallTurnLogController log)
    {
        PlayerResourceInventory inv = GameManager.Instance.PlayerInventory;
        if (!inv.ConsumeItem(ItemIds.StoneSpear, 1))
        {
            log.AddLog($"[{smallTurn} 턴] {owner.Data.Name}은/는 돌창이 없습니다.");
            return;
        }

        owner.EquipWeaponWithSwap(WeaponType.StoneSpear, inv);
        log.AddLog($"[{smallTurn} 턴] {owner.Data.Name}은/는 돌창을 착용합니다.");
    }

    public void RunEquipFanTurn(CharacterEntity owner, int smallTurn, SmallTurnLogController log)
    {
        PlayerResourceInventory inv = GameManager.Instance.PlayerInventory;
        if (!inv.ConsumeItem(ItemIds.Fan, 1))
        {
            log.AddLog($"[{smallTurn} 턴] {owner.Data.Name}은/는 부채가 없습니다.");
            return;
        }

        if (!owner.Equipment.TryEquipUtility(UtilityType.Fan))
        {
            inv.AddItem(ItemIds.Fan, 1);
            log.AddLog($"[{smallTurn} 턴] {owner.Data.Name}은/는 이미 부채를 착용 중입니다.");
            return;
        }

        log.AddLog($"[{smallTurn} 턴] {owner.Data.Name}은/는 부채를 착용합니다.");
    }

    public void RunUseBandageTurn(CharacterEntity owner, int smallTurn, SmallTurnLogController log)
    {
        PlayerResourceInventory inv = GameManager.Instance.PlayerInventory;
        if (!inv.ConsumeItem(ItemIds.Bandage, 1))
        {
            log.AddLog($"[{smallTurn} 턴] {owner.Data.Name}은/는 붕대가 없습니다.");
            return;
        }

        owner.Status.AddHealth(20f, owner.Data);
        log.AddLog($"[{smallTurn} 턴] {owner.Data.Name}은/는 붕대를 사용합니다.");
    }

    public void RunUseMedkitTurn(CharacterEntity owner, int smallTurn, SmallTurnLogController log)
    {
        PlayerResourceInventory inv = GameManager.Instance.PlayerInventory;
        if (!inv.ConsumeItem(ItemIds.Medkit, 1))
        {
            log.AddLog($"[{smallTurn} 턴] {owner.Data.Name}은/는 구급상자가 없습니다.");
            return;
        }

        owner.Status.AddHealth(50f, owner.Data);
        log.AddLog($"[{smallTurn} 턴] {owner.Data.Name}은/는 구급상자를 사용합니다.");
    }
}
