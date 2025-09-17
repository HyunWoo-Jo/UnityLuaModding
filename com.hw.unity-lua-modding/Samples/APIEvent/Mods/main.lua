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
    
    Event:Subscribe("TestEvent", function(data) 
        print("Event 발생 (람다)")
    end)
    Event:Subscribe("TestEvent", TestMod.Event) 

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

function TestMod.Event()
    print("Event 발생 (함수)")
end

-- 모드 테이블 반환 (중요!)
return TestMod