local ItemData = {}

-- item info table
ItemData.items = {
    ["sword"] = {
        name = "sword",
        path = "UI/sword.png",
        effect = function(player)
            print("attack!")
        end
    },

    ["shield"] = {
        name = "shield",
        path = "UI/shield.png",
        effect = function(player)
            print("block!")
        end
    },
    
    ["potion"] = {
        name = "hp potion",
        path = "UI/potion.png",
        effect = function(player)
            print("recoveryhp")
        end
    },
}

function ItemData:GetItem(itemName)
    return self.items[itemName]
end

function ItemData:ItemExists(itemName)
    return self.items[itemName] ~= nil
end

function ItemData:UseItem(itemName, owner)
    local item = self:GetItem(itemName)
    if item and item.effect then
        item.effect(owner)
        return true
    end
    return false
end

return ItemData