#!/usr/bin/python3
 

def ParamTime(strShortTime):
    timeList = strShortTime.split(":")
    if(len(timeList) != 3):
        print("timeList!=3: ", strShortTime)

    hour = (int)(timeList[0])
    m = (int)(timeList[1])
    s = (int)(timeList[2])

    return hour *3600 + m * 60 + s

class DecItem:
    userGuid = 0
    serverId = 0
    decItemNum = 0
    decItemId = 0
    decTime = 0
    decShortTime = 0
    line = ""
    
    
    def PrintValue(self):
        print(self.line)
        print(self.userGuid)
        print(self.serverId)
        print(self.decShortTime)
        print(self.decTime)


class LevelUpBuilding:
    userGuid = 0
    serverId = 0
    decTime = 0
    decShortTime = 0
    line = ""
    
    def PrintValue(self):
        print(self.line)
        print(self.userGuid)
        print(self.serverId)
        print(self.decShortTime)
        print(self.decTime)

def ReadItemFile(itemList):
    fo = open(R"F:\item.csv", "r",encoding='gb18030',errors='ignore')
    print ("file:", fo.name)
    itemList = []
    testCt = 0
    for line in fo.readlines():                          
        line = line.strip()                            
        retSpace = line.split(" ")
        if(len(retSpace) != 2):
            print ("len != 2: %s" % (line))
            continue
        
        strTimeList = retSpace[0].split(",")
        if(len(strTimeList) != 4):
            print ("len(strTimeList) != 4: %s" % (retSpace[0]))
            continue
            
        strDataList = retSpace[1].split(",")
        if(len(strDataList) < 25):
            print ("len(strDataList) < 25: %s" % (retSpace[1]))
            continue


        serverId = strTimeList[2]
        time = strTimeList[3] 

        decShortTime = strDataList[0]
        userguid = strDataList[5]
        decItemId = strDataList[15]
        decItemNum = strDataList[16]

        if(time>="2019-08-24"):
            continue



        di = DecItem()
        di.userGuid = userguid
        di.serverId = serverId
        di.decTime = time
        di.decShortTime = ParamTime(decShortTime)
        di.line = line
        di.decItemId = decItemId
        di.decItemNum = decItemNum

        #di.PrintValue()

        itemList.append(di)


        #testCt = testCt+1
        #if(testCt>10):
         #   break

    fo.close()
    return itemList



def ReadHomeFile(itemList):
    fo = open(R"F:\home.csv", "r",encoding='gb18030',errors='ignore')
    print ("file: ", fo.name)
    itemList = []
    testCt = 0
    for line in fo.readlines():                     
        line = line.strip()                            
        retSpace = line.split(" ")
        if(len(retSpace) != 2):
            print ("len != 2: %s" % (line))
            continue
        
        strTimeList = retSpace[0].split(",")
        if(len(strTimeList) != 4):
            print ("len(strTimeList) != 4: %s" % (retSpace[0]))
            continue
            
        strDataList = retSpace[1].split(",")
        if(len(strDataList) < 17):
            print ("len(strDataList) < 17: %s" % (retSpace[1]))
            continue
           


        serverId = strTimeList[2]
        time = strTimeList[3] 

        decShortTime = strDataList[0]
        userguid = strDataList[5]


        if(time>="2019-08-24"):
            continue



        di = LevelUpBuilding()
        di.userGuid = userguid
        di.serverId = serverId
        di.decTime = time
        di.decShortTime = ParamTime(decShortTime)

        #di.PrintValue()

        itemList.append(di)


        #testCt = testCt+1
        #if(testCt>10):
         #   break

    fo.close()
    return itemList


def IsSameTime(time1, time2):
    return  (abs(time1-time2) <2)
    

    
def __Main():
    print("Main")

    itemList = []

    itemList = ReadItemFile(itemList)

    lvUpList = []

    lvUpList = ReadHomeFile(lvUpList)

    missItem = 0

    fw = open(R"F:\result_detail.txt", "w", encoding='gb18030',errors='ignore')
    for item in itemList:
        bFind = False
        for lvup in lvUpList:
            if(lvup.userGuid == item.userGuid and IsSameTime(lvup.decShortTime,item.decShortTime)):
                bFind = True
                break

        if(bFind == False):
            missItem = missItem +1
            #item.PrintValue()
            fw.write("需补偿玩家数据:" + str(missItem) + "\n")
            fw.write("服务器ID:" + str(item.serverId) + "\n")
            fw.write("玩家GUID:" + str(item.userGuid) + "\n")
            fw.write("物品ID:" + str(item.decItemId) + "\n")
            fw.write("数量:" + str(item.decItemNum) + "\n")

    print("missItem:", missItem)
    print(len(itemList))
    print(len(lvUpList))
    fw.close()

__Main();