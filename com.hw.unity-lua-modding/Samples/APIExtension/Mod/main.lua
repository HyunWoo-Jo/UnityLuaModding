-- 테스트 Lua 모드
local TestMod = {}

TestMod.name = "Test Lua Mod"
TestMod.version = "1.0"

function TestMod.Initialize(context)
    print("=== Lua Mod 초기화 ===")
    print("Mod Name: " .. TestMod.name)
    print("Version: " .. TestMod.version)
    
    -- 컨텍스트 저장
    TestMod.context = context
    
    ModAPI:ExtensionDebug()
    
    local player = ModAPI:GetPlayer()
    if player ~= nil then 
        ModAPI:CreateUI(player)
    end
    print("=== Mod Susucess ===")
    return true
end

function TestMod.Update(deltaTime)

end

function TestMod.OnPause()
    print("Lua Mod Puase")
end

function TestMod.OnResume()
    print("Lua Mod Resume")
end

function TestMod.Shutdown()
    print("=== Lua Mod Shutdown ===")
end



-- 모드 테이블 반환 (중요!)
return TestMod