-- Sample Inventory Mod
local TestMod = {}
local ItemData

TestMod.name = "Sample Lua Inventory Mod"
TestMod.version = "1.0"

local inventoryObject
local isInventoryOpen = false

local uiTable = {}

local inventory = {}
local maxSlots = 9


function TestMod.Initialize(context)
    -- save context
    TestMod.context = context
    ItemData = LoadLua("data/items.lua")

    TestMod.CreateInventoryUI()

    return true
end

function TestMod.Update(deltaTime)
    -- show/hide ui
    if Unity:GetKeyDown("i") then
        TestMod:ToggleInventory()
    end

    -- selete slot
    for i = 1, 9 do
        if Unity:GetKeyDown(tostring(i)) then
            selectedSlot = i
            if isInventoryOpen then
                TestMod:ShowInventory()
            end
        end
    end

    -- Add item
    if Unity:GetKeyDown("space") then
        TestMod:AddRandomItem()
    end
end

function TestMod.OnPause()

end

function TestMod.OnResume()
  
end

function TestMod.Shutdown()

end

function TestMod.SceneChanged(sceneName)

end

function TestMod:CreateInventoryUI()
    local mainCanvas = ModAPI:GetMainCanvas()
    inventoryObject = ModAPI:CreateUI("UI/inventory_parent_ui.json", mainCanvas.transform)
    for i = 1, 9 do
        local uiName = "slot"..tostring(i)
        local slotObject = ModAPI:CreateUI("UI/inventory_slot.json", inventoryObject.transform, uiName)
        local x = (i - 5) * 160
        ModAPI:SetRectPosition(slotObject, x, 100)
        uiTable[uiName] = slotObject
    end
    
    ModAPI:SetActive(inventoryObject, false)
end

function TestMod:ToggleInventory()
    isInventoryOpen = not isInventoryOpen
    
    if isInventoryOpen then
        TestMod:ShowInventory()
    else
        TestMod:HideInventory()
    end
end
-- Show ui
function TestMod:ShowInventory()    
    ModAPI:SetActive(inventoryObject, true)
end

-- Hide ui
function TestMod:HideInventory()
    ModAPI:SetActive(inventoryObject, false)
   
end

function TestMod:AddRandomItem()

    
    local random = Unity:RandomInt(0,3)
    local itemName
    if random == 0 then
        itemName = "potion"
    elseif random == 1 then
        itemName = "shield"
    else
        itemName = "sword"
    end
    TestMod:CreateItem(itemName)
end

function TestMod:CreateItem(itemName)
    local emptySlot = TestMod:FindEmptySlot()
    if not emptySlot then
        print("Inventory is full!")
        return false
    end

    local item = ItemData:GetItem(itemName)
    if not item then
        print("Item not found: " .. itemName)
        return false
    end
    local slotUI = uiTable["slot" .. emptySlot]
    local itemUI = ModAPI:CreateUI("UI/item_ui.json", slotUI.transform, item.name.."_ui")
    ModAPI:SetSprite(itemUI, item.path)
    ModAPI:SetButtonEvents(itemUI, "PointerDown", "ClickItem", {tostring(emptySlot)})
    inventory[emptySlot] = {
        name = itemName,
        item = item,
        ui = itemUI
    }
    return true
end
function TestMod:FindEmptySlot()
    for i = 1, maxSlots do
        if not inventory[i] then
            return i
        end
    end
    return nil 
end

function TestMod.ClickItem(param)
    local index = tonumber(param);
    ItemData:UseItem(inventory[index].name)
end

-- return mod Table (important)
return TestMod