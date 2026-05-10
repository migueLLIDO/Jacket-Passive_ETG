using BepInEx;
using UnityEngine;
using Alexandria.ItemAPI;


namespace JacketPassiveEnhancement
{
    [BepInPlugin("jacket.passive.aesthetic", "Jacket Aesthetic of Vice", "2.0.1")]
    public class JacketPassiveEnhancement : BaseUnityPlugin
    {
        private bool foundJacket = false;
        private PlayerController jacketPlayer;
        private bool itemAdded = false;
        
        private int currentStacks = 0;
        private float stackTimer = 0f;
        private float lastHealth = 1f;
        
        private const float DAMAGE_PER_STACK = 0.01f;
        private const float SPEED_PER_STACK = 0.005f;
        private const float RELOAD_PER_STACK = 0.01f;
        private const int MAX_STACKS = 25;
        private const float STACK_DURATION = 5f;
        
        private float originalDamage = 1f;
        private float originalSpeed = 1f;
        private float originalReload = 1f;
        
        void Awake()
        {
            Logger.LogInfo("✅ Aesthetic of Vice - Mod Carregado!");
            
            // Registra o item passivo
            RegisterPassiveItem();
        }
        
private void RegisterPassiveItem()
{
    try
    {
        string itemName = "Aesthetic of Vice";
        string resourceName = "JacketPassiveEnhancement.src.icon";
        
        GameObject obj = new GameObject(itemName);
        var item = obj.AddComponent<AestheticOfViceItem>();
        
        ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);
        
        string shortDesc = "Violence is the Answer";
string longDesc = "A red jacket covered in blood, worn by one of Miami's most relentless killers.\n\n" +
                  "They say whoever wears it feels an uncontrollable fury and a connection to chaos. The original owner cleared entire buildings alone, leaving only trails of carnage behind.\n\n" +
                  "Effect: Aesthetic of Vice\n" +
                  "Kill enemies quickly to build stacks!\n\n" +
                  "Each stack grants:\n" +
                  "• +1% Damage (max 25%)\n" +
                  "• +0.5% Speed (max 12.5%)\n" +
                  "• +1% Reload Speed (max 25%)\n\n" +
                  "Getting hit removes ALL stacks!\n\n" +
                  "Hotline Miami style!";
        
        ItemBuilder.SetupItem(item, shortDesc, longDesc, "jacket");
        // NÃO use SetupUnlockOnCustomFlag - essa linha NÃO existe na API
        
        item.quality = PickupObject.ItemQuality.A;
        
        Logger.LogInfo("✅ Item 'Aesthetic of Vice' registrado com sucesso!");
    }
    catch (System.Exception e)
    {
        Logger.LogError($"Erro ao registrar item: {e.Message}");
    }
}

private bool isNewRun = true;





    void Update()
{
    // Detecta Quick Restart
    if (GameManager.Instance != null && GameManager.Instance.IsLoadingLevel)
    {
        isNewRun = true;
        foundJacket = false;
        jacketPlayer = null;
        itemAdded = false;
        Logger.LogInfo("🔄 Nova run detectada! Resetando...");
        return;
    }

    if (!foundJacket)
    {
        FindJacket();
    }
    else if (jacketPlayer != null)
    {
        UpdateStackTimer();
        CheckForDamage();
        UpdateBonuses();
        
        // Verifica se o jogador ainda existe (não morreu)
        if (jacketPlayer.healthHaver == null || jacketPlayer.healthHaver.GetCurrentHealth() <= 0)
        {
            foundJacket = false;
            jacketPlayer = null;
            itemAdded = false;
            Logger.LogInfo("💀 Jacket morreu! Aguardando respawn...");
        }
    }
}
        
        private void FindJacket()
        {
            try
            {
                PlayerController player = (PlayerController)GameObject.FindObjectOfType(typeof(PlayerController));
                
                if (player != null && IsJacket(player))
                {
                    ActivatePassive(player);
                }
            }
            catch (System.Exception e)
            {
                Logger.LogError($"Erro: {e.Message}");
            }
        }
        
        private bool IsJacket(PlayerController player)
        {
            if (player.name.ToLower().Contains("jacket"))
                return true;
                
            if (player.CurrentGun != null)
            {
                string gunName = player.CurrentGun.name.ToLower();
                if (gunName.Contains("bat") || gunName.Contains("taco"))
                    return true;
            }
            
            return false;
        }
        
        private void ActivatePassive(PlayerController player)
        {
            jacketPlayer = player;
            foundJacket = true;
            
            // Salva stats originais
            originalDamage = jacketPlayer.stats.GetStatValue(PlayerStats.StatType.Damage);
            originalSpeed = jacketPlayer.stats.GetStatValue(PlayerStats.StatType.MovementSpeed);
            originalReload = jacketPlayer.stats.GetStatValue(PlayerStats.StatType.ReloadSpeed);
            
            // Salva vida inicial
            lastHealth = jacketPlayer.healthHaver.GetCurrentHealth();
            
            // Registra evento de kill
            try
            {
                jacketPlayer.OnKilledEnemy += OnEnemyKilled;
            }
            catch
            {
                Logger.LogInfo("⚠️ Evento OnKilledEnemy não disponível");
            }
            
            // Adiciona o item ao inventário
            AddItemToInventory();
            
            Logger.LogInfo("🎭 Aesthetic of Vice ATIVADA!");
            Logger.LogInfo($"📊 Máximo: {MAX_STACKS} stacks | Duração: {STACK_DURATION}s");
        }
        
        private void AddItemToInventory()
        {
            if (itemAdded) return;
            
            try
            {
                var item = PickupObjectDatabase.GetByName("Aesthetic of Vice") as PassiveItem;
                if (item != null)
                {
                    item.Pickup(jacketPlayer);
                    itemAdded = true;
                    Logger.LogInfo("📦 Item 'Aesthetic of Vice' adicionado ao inventário!");
                }
                else
                {
                    Logger.LogError("❌ Item não encontrado na database!");
                }
            }
            catch (System.Exception e)
            {
                Logger.LogError($"Erro ao adicionar item: {e.Message}");
            }
        }
        
        private void OnEnemyKilled(PlayerController player)
        {
            if (player != jacketPlayer) return;
            AddStack();
        }
        
        private void CheckForDamage()
        {
            if (jacketPlayer == null || jacketPlayer.healthHaver == null) return;
            
            float currentHealth = jacketPlayer.healthHaver.GetCurrentHealth();
            
            if (currentHealth < lastHealth && currentStacks > 0)
            {
                Logger.LogInfo($"💥 RESET! Perdeu todos os {currentStacks} stacks ao tomar dano!");
                currentStacks = 0;
                stackTimer = 0;
            }
            
            lastHealth = currentHealth;
        }
        
        private void AddStack()
        {
            if (currentStacks < MAX_STACKS)
            {
                currentStacks++;
                stackTimer = STACK_DURATION;
                
                Logger.LogInfo($"🔥 Stack +1/{MAX_STACKS}!");
                Logger.LogInfo($"   Dano: +{currentStacks * DAMAGE_PER_STACK * 100:F1}%");
                Logger.LogInfo($"   Velocidade: +{currentStacks * SPEED_PER_STACK * 100:F1}%");
                Logger.LogInfo($"   Recarga: +{currentStacks * RELOAD_PER_STACK * 100:F1}%");
            }
            else
            {
                stackTimer = STACK_DURATION;
                Logger.LogInfo($"⚡ MAX STACKS! ({MAX_STACKS}/{MAX_STACKS}) - Mantendo bônus máximos!");
            }
        }
        
        private void UpdateStackTimer()
        {
            if (currentStacks > 0 && stackTimer > 0)
            {
                stackTimer -= Time.deltaTime;
                if (stackTimer <= 0)
                {
                    currentStacks = 0;
                    Logger.LogInfo($"⏰ Todos os stacks expiraram!");
                }
            }
        }
        
        private void UpdateBonuses()
        {
            if (jacketPlayer == null) return;
            
            float damageMult = 1f + (currentStacks * DAMAGE_PER_STACK);
            float speedMult = 1f + (currentStacks * SPEED_PER_STACK);
            float reloadMult = 1f + (currentStacks * RELOAD_PER_STACK);
            
            jacketPlayer.stats.SetBaseStatValue(PlayerStats.StatType.Damage, originalDamage * damageMult, jacketPlayer);
            jacketPlayer.stats.SetBaseStatValue(PlayerStats.StatType.MovementSpeed, originalSpeed * speedMult, jacketPlayer);
            jacketPlayer.stats.SetBaseStatValue(PlayerStats.StatType.ReloadSpeed, originalReload * reloadMult, jacketPlayer);
            jacketPlayer.stats.RecalculateStats(jacketPlayer);
        }
    }
    
  // Classe do Item Passivo - Versão mínima
public class AestheticOfViceItem : PassiveItem
{
    // Sem override do Drop - isso já impede que seja dropado naturalmente
}}
