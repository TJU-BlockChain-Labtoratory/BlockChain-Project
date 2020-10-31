# Read-me

​    本团队拟利用aelf区块链实现存证。

// 以太坊上的一个有趣的erc721应用，以太猫
// 合约地址：https://etherscan.io/token/0x06012c8cf97bead5deae237070f9587f8e7a266d#readContract

### 1. NFT合约：版权资产证明合约 (CopyRightTokenContract)

- ##### 定义：每一个存储在链上的版权都被唯一的映射到对应的资产证明token，即CRT中。

- ##### NFT：版权资产证明CopyRightToken(CRT)

- ##### 身份介绍：

  - 创作者：版权作品的创作者，拥有版权的人身权。即该作品为此人创作。此属性不可被任何人侵犯与篡改。
  - 所有者：即拥有该版权财产权的人，可以对该版权随意处分，也可利用该版权进行获得利益。
  - 被许可人：即被许可使用该版权的人。他仅可在合同限制的范围内使用被授予的作品权利。

- ##### message CRT：

  - CRT_Info：数据结构（下文所描述的），代表着CRT的基本信息
  - CRT_Authorized：地址数组，标识被授权使用该Token的全部用户（被许可人）。
  - CRT_History：地址数组，记录所有与该Token相关的交易的ID。（是否需要仍需考虑） // 其实不要写在区块链上，链外记录就行了。需要的时候，用链外的txID到链上查询真伪就行
  - CRT_Approved：地址数组，记录所有有权修改该CRT属性（除去只读属性）的用户（合约本身也需要Approve后才能操作属性）

- ##### message CRT_Info：

  - CRT_ID：只读，唯一标识，被用于标识对应版权。
  - CRT_Creator：只读，地址，标识此Token所对应作品的创造者。
  - CRT_Owner：地址，标识此Token财产权的所有者。
  - CRT_Content：只读，地址，指向该token对应作品数据所存储的链接位置。
  - CRT_Status：状态属性，暂时包括正常、被质押和被销毁。
  - CRT_Pledge_Info：地址，仅在CR_Status为被质押时有效。存储版权被质押信息的交易ID。

- ##### message CRT_User： <!--暂时不实现，仅用地址代替-->
// 这一部分数据，建议存在链外。数据量小的话，可以考虑存在链上。但是做链外的话，有额外的开发量，你们自己决定咯。
// 用户是否拥有该CRT，可以考虑用类似 CRT.getBalance(user_address) 的方式来验证
// 另外，approve这个操作。 一般是地址A授权地址B可以使用地址A的资产<!--User_Approved是当地址A授权地址B可以使用地址A的资产A时，地址B进行记录的-->

  - User_Address：只读，唯一标识，存储用户地址
  - User_Owned：ID数组，记录其拥有的所有CRT
  - User_Authorized：ID数组，记录其被授权的所有CRT
  - User_Approved：ID数组，记录其有权操作的所有CRT
  - User_Status：状态属性，仍需进一步设计

- ##### 合约内部私有函数<!--暂时不实现，仅用地址代替-->

  - ##### User_addCRT(address addr, uint256 CRT_ID, int flag)

    - 用于addr所对应的CRT_User中添加CRT
    - 如果在CRT不存在，flag非法，addr所对应的CRT_User不存在或未登录这四种情况下，则返回输入错误码3。
    - 根据flag的不同，添加至addr所对应CRT_User的不同的数组（0：成为其所有者。1：成为其被许可人。2：获得操作权限）
    - 如果成功返回0，否则返回系统错误码1。

  - ##### User_delCRT(address addr, uint256 CRT_ID, int flag)

    - 用于从addr所对应的CRT_User中删除CRT
    - 如果在CRT不存在，flag非法，addr所对应的CRT_User不存在或未登录这四种情况下，则返回输入错误码3。
    - 根据flag的不同，从addr所对应CRT_User的不同的数组中移除（0：不再是所有者。1：不再是被许可人。2：失去操作权限）。如果对应数组中不存在对应项目，则返回输入错误码3。
    - 如果成功返回0，否则返回系统错误码1。

- ##### rpc函数：

  - ##### CRT_Create(address CRT_Creator, address CRT_Owner, uint64 CRT_Status, uint256 CRT_Content)
    
    - 用于创建新的CRT。
    - 在该method中，认为CRT_Content已经被检查过（在上层合约中被检查），为可以申请版权，非侵权且未计入系统的作品；同时CRT_Owner也未合法身份（同样在上层合约被检查）
    - 根据上述属性生成新CRT。
    - 调用User_addCRT()，修改CRT_Owner所对应的CRT_User的状态。
    - 如果成功则返回CRT_ID，否则返回0。
    
  - ##### CRT_Approve(address addr, uint256 CRT_ID)
    
    - 用于授予addr地址CRT_ID所对应的CRT的修改权。
    - 仅能由该CRT_ID所对应CRT中CRT_Info的CRT_Owner发出，任何其他人发出将被拒绝，并返回身份错误码2。
    - 修改CRT_ID所对应CRT的CRT_Approved数组，将addr添加进数组中。
    - 调用User_addCRT()，修改CRT_Owner所对应的CRT_User的状态。
    - 如果成功则返回0，否则返回系统错误码1。
    
  - ##### CRT_UnApprove(address addr, uint256 CRT_ID)
    
    - 用于解除已经授予addr地址的CRT_ID所对应的CRT的修改权。
    - 仅能由该CRT_ID所对应CRT中CR_Info的CRT_Owner发出，任何其他人发出将被拒绝。
    - 修改CRT_ID所对应CRT的CRT_Approved数组，将addr项剔除。
    - 调用User_addCRT()，修改CRT_Owner所对应的CRT_User的状态。
    - 如果成功则返回0，否则返回系统错误码1。
    
  - ##### CRT_Pledge(address addr , Hash txID, uint256 CRT_ID)
    
    - 用于质押版权。
    - 仅能由该CRT_ID所对应CRT中CRT_Info的CRT_Owner或在CRT_Approved中的用户发出，任何其他人发出将被拒绝，并返回身份错误码2。。
    - 修改CRT_ID所对应CRT中CRT_Info的CRT_Status成被质押，将CRT_Pledge_Info的值修改成txID，将CRT_Owner修改成addr。
    - 调用User_addCRT()，修改CRT_Owner所对应的CRT_User的状态。
    - 如果成功则返回0，否则返回系统错误码1。
    
  - ##### CRT_UnPledge(address addr,uint256 CRT_ID)
    
    - 用于取消质押版权。如果addr与执行CRT_Pledge之前的CRT_Owner相同，则是撤销质押（可能是赎回）；如果不同则是被收取质押的一方处置了。
    - 仅能由该CRT_ID所对应CRT中CRT_Info的CRT_Owner或在CRT_Approved中的用户发出，任何其他人发出将被拒绝，并返回身份错误码2。。
    - 修改CRT_ID所对应CRT中CRT_Info的CRT_Status成正常，将CRT_Pledge_Info的值清空，将CRT_Owner修改成addr。
    - 调用User_addCRT()，修改CRT_Owner所对应的CRT_User的状态。
    - 如果成功则返回0，否则返回系统错误码1。
    
  - ##### CRT_Authorize(address addr, uint256 CRT_ID)
    
    - 用于授权版权
    - 仅能由该CRT_ID所对应CRT中CRT_Info的CRT_Owner或在CRT_Approved中的用户发出，任何其他人发出将被拒绝，并返回身份错误码2。。
    - 修改CRT_ID所对应CRT的CRT_Authorized数组，将addr添加进数组中。
    - 调用User_addCRT()，修改CRT_Owner所对应的CRT_User的状态。
    - 如果成功则返回0，否则返回系统错误码1。
    
  - ##### CRT_UnAuthorize(adress addr, uint256 CRT_ID)
    
    - 用于取消授权。
    - 仅能由该CRT_ID所对应CRT中CRT_Info的CRT_Owner或在CRT_Approved中的用户发出，任何其他人发出将被拒绝，并返回身份错误码2。。
    - 修改CRT_ID所对应CRT的CRT_Authorized数组，将addr项剔除。
    - 调用User_addCRT()，修改CRT_Owner所对应的CRT_User的状态。
    - 如果成功则返回0，否则返回系统错误码1。
    
  - ##### CRT_ChangeOwner(address addr, uint256 CRT_ID)
    
    - 用于更换版权所有者
    - 仅能由该CRT_ID所对应CRT中CRT_Info的CRT_Owner或在CRT_Approved中的用户发出，任何其他人发出将被拒绝，并返回身份错误码2。。
    - 修改CRT_ID所对应CRT中CRT_Info的CRT_Owner为addr。
    - 调用User_addCRT()，分别修改CRT_Owner与addr所对应的CRT_User的状态。
    - 如果成功则返回0，否则返回系统错误码1。
    
  - ##### CRT_Log(Hash txID, uint256 CRT_ID)
    
    - 用于写入历史纪录。
    - 仅能由合约本身调用，其他人调用将被拒绝。
    - 使用时需要检查txID对应的交易是否已经被写入，若交易状态异常则拒绝操作。
  - 修改CRT_ID所对应CRT的CRT_History数组，将blkID添加进数组中。
    
    - 如果成功则返回0，否则返回系统错误码1。
    
  - ##### CRT_Destory(uint256 CRT_ID)
    
    - 用于销毁该版权记录。
    - 仅能由该CRT_ID所对应CRT中CRT_Info的CRT_Owner或在CRT_Approved中的用户发出，任何其他人发出将被拒绝，并返回身份错误码2。
    - 检测该CRT_ID所对应CRT的CRT_Authorized是否为空以及CRT_Status是否为正常，如果数组不为空或者状态异常，则返回系统错误码1。
    - 修改CRT_ID所对应CRT中CRT_Info的CRT_Status为被销毁。
    - 调用User_addCRT()，修改CRT_Owner所对应的CRT_User的状态。
    - 此后，该CRT变为只读状态，且所有原持有者(CRT_Owner、CRT_Authorized、CRT_Creater)试图证明对其具有任何权利的行为均被拒绝。
    - 如果成功则返回0，否则返回系统错误码1。
    

 // 这一批用户操作的意义是？ 
 // 如果是为了减少demo开发成本，快速获取账户，倒还是可以。
 // 在区块链的历史记录里，可以知道一个地址是否有 CRT，有哪些授权。

  - ##### User_Create(address addr)

    - 用于创建新的用户
    - 如果已经存在该用户，则返回输入错误码3.
    - 初始所有数组均为空，状态为未登录
    - 如果成功返回0，否则返回系统错误码1

  - ##### User_Delete(address addr)

    - 用于注销用户
    - 如果不存在，则返回输入错误码3。
    - 删除addr所对应的CRT_User
    - 如果成功返回0，否则返回系统错误码1

  - ##### User_Login(address addr)

    - 用于用户登录
    - 如果该用户不存在或者状态异常（如已登录），则返回输入错误码3。
    - 修改addr所对应的CRT_User中的状态为已登录。
    - 如果成功则返回0，否则返回系统错误码1。

  - ##### User_LogOut(address addr)

    - 用于用户登出
    - 如果该用户不存在或者状态异常（如未登录），则返回输入错误码3。
    - 修改addr所对应的CRT_User中的状态为为登录。
    - 如果成功则返回0，否则返回系统错误码1。

- ##### rpc view函数：

  - ##### isApproved(address addr , uint256 CRT_ID)
    
    - 用于获取状态，判断addr是否拥有对CRT_ID所对应CRT的操作权
    - 如果CRT不存在则返回假。
    - 如果addr在CRT_ID所对应CRT的CRT_Approved数组中，则返回真，否则返回假
    
  - ##### isAuthorized(address addr , uint256 CRT_ID)
    
    - 用于获取状态，判断addr是否是CRT_ID所对应CRT的被许可人
    - 如果CRT不存在则返回假。
    - 如果addr在CRT_ID所对应CRT的CRT_Approved数组中，则返回真，否则返回假
    
  - ##### getAllInfo(uint256 CRT_ID)
    
    - 用于获取CRT_ID所对应的CRT的基本信息
    - 如果CRT不存在则返回空的CRT_Info。
    - 返回CRT_Info
    
  - ##### getHistory(uint256 CRT_ID)
    
    - 用于获取CRT_ID所对应CRT的历史
    - 如果CRT不存在则返回空的CRT_Info。
    - 返回CRT_History数组
    
  - ##### getPledgeInfo(uint256 CRT_ID)

    - 用于获取CRT_ID所对应CRT的质押信息
    - 如果CRT不存在则返回空的地址。
    - 返回CRT_Pledge_Info。

  - ##### getUserInfo(address addr)

    - 用于获取addr所对应CRT_User的全部信息
    - 如果addr所对应CRT_User不存在则返回空的CRT_User
    - 返回CRT_User。

### 2. 具体实现：CopyRightContract

- ##### 综述：本合约初步需要实现存证上链和存证转移的核心功能，之后实现存证的验证（即文件比对）。为更好实现功能，需要额外添加注册和登录功能。同时，版权的质权转移和授权操作也暂时延后。

- ##### 功能描述：

  - ##### 存证上链（CR_Upload）：即生成新版权的过程。只能由将来的版权所有者进行申请（所有者和创作者可为同一人，但不一定是创作者来申请版权）
    
    - 输入为 创作者CRT_Creator，所有者CRT_Owner，版权状态CRT_Status，版权内容 CRT_Content和手续费Fee
    - 输出为结果信息（整形），如果成功则为0，如果出现任何问题，则返回对应错误码。
    
  - ##### 存证转移（CR_Transfer）：即版权转让的过程，只能由版权所有者进行，将版权的所有者改成新的版权所有者。<!--注意，本系统假设只有当所有者收回所有授权（或从未授权），且不存在任何正在生效的质押行为时才能进行版权转让，不然会涉及到版权被许可人和质押人的权利问题。该假设需要根据进一步的调研来进行更改-->
    
    - 输入为新所有者地址addr，CRT标识CRT_ID以及交易金额Price。
    - 输出为结果信息（整形），如果到此没有任何问题则返回0。
    
  - ##### 存证验证（CR_Confirm）：验证脸上是否存在重复或者类似存证（防止侵权），尚未设计完成。

  - ##### 登录（CR_Login）、注册（CR_Register）、登出（CR_Logout）和注销（CR_Delete）

    - 注册：利用用户地址进行注册
    - 登录：利用用户地址进行登录
    - 登出：利用用户地址进行登出
    - 注销：利用用户地址进行注销
    
  - ##### 存证授权（CR_Authorize）：即版权授权过程，只能由版权所有者进行，为版权添加新的被许可人。尚未涉及完成。

  - ##### 存证质押（CR_Pledge）:即版权授权过程，只能由版权所有者进行，将版权转移至质押人手中，并设置质押合同信息。尚未涉及完成。

- ##### 合约内部逻辑

  - ##### CR_Upload（需要先置的approve交易）：

    - 输入创作者CRT_Creator，所有者CRT_Owner，版权状态CRT_Status，版权内容 CRT_Content和手续费Fee。
    - 验证版权是否合法（较为模糊，暂且跳过）
    - 验证身份，如果Context.Sender != CRT_Owner，则退出，并返回身份错误码2。
    - 其他验证，验证CRT_Creater、CRT_Status和CRT_Content是否合法，如果存在问题则返回输入错误码3。
    - 调用CopyRightTokenContract中的CRT_Create，用于生成新Token。如果返回的CRT_ID==0则表示操作失败，返回错误码1。
    - 调用MultiTokenContract用于转移版权申请办理的手续费。如果交易失败（状态不是Mined）则返回错误码1。
    - 输出为结果信息（整形），如果到此完全没有问题则为0。

  - ##### CR_Transfer（需要先置的approve交易）： 

    - 输入为新所有者地址addr，CRT标识CRT_ID以及交易金额Price。
    - 首先发出CRT_getOwner()，获取当前所有者old_addr并与调用者(Context.Sender)进行比较，如果不同则拒绝操作，返回身份错误码2。
    - 然后发出CRT_Approve()，授予合约操控CRT的权利，如果失败则退出，返回系统错误码1。
    - 在此之后，一旦退出则一定要发出CRT_UnApprove，防止赋予合约过多的权利。
    - 合约发出TransferFrom()，将金额Price从addr转至old_addr。如果失败则退出，返回系统错误码1。
    - 合约发出CR_ChangeOwner()，修改CRT信息。如果失败则退出，返回系统错误码1。
    - 输出为结果信息（整形），如果到此没有任何问题则返回0。
    - <!--理论上来讲，从Approve开始直到结束为单个原子操作。不可打断，一旦出错直接回滚。但暂时没想到如何实现回滚（或许有API？）-->

  - ##### CR_Register：

    - 输入为用户地址
    - 调用User_Create()
    - 输出为布尔值，成功则为true
    
  - ##### CR_Login：

    - 输入为用户地址
    - 调用User_Login()
    - 输出为布尔值，成功则为true
    
  - ##### CR_Logout：

    - 输入为用户地址
    - 调用User_Logout()
    - 输出为布尔值，成功则为true

  - ##### CR_Delete：

    - 输入为用户地址
    - 调用User_Delete()
    - 输出为布尔值，成功则为true
