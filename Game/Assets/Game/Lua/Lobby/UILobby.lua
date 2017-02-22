-- UILobby.lua
-- Author : Dzq
-- Date : 2017-02-22
-- Last modification : 2017-02-22
-- Desc: 大厅

module(...,package.seeall)

local item ={} 
item.value = UnityEngine.UI.Text
item.receive= UnityEngine.GameObject
item.light= UnityEngine.GameObject
item.icon= UnityEngine.UI.Image
item.btn= UnityEngine.UI.Button 
local itemDefine = util.Clone(item)

util.GetMonoTable(activeGroup:Get(1),itemDefine,item) --activeGroup 是UIGroup

item.value.text = tostring(cfg.activenessRequire)
UI.UIHelper.SetSprite(item.icon,iconName) 

function Init()
	
end

function Open()
	
end

function Fresh()
	
end

function Close()
	
end



function OnOpen()

end

function OnClose()
	
end

function OnUpdate()
	
end

function OnFresh()
	
end

function OnDestroy()

end