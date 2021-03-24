/**
 * @file index.js
 * @author zmh3788
 * @description none
*/

import AElf from 'aelf-sdk';

const { sha256 } = AElf.utils;

const defaultPrivateKey = '845dadc4609852818f3f7466b63adad0504ee77798b91853fdab6af80d3a4eba';
const CRAddress = '2LUmicHyH4RXrMjG4beDwuDsiWJESyLkgkwPdGTR8kahRzq5XS';
// const wallet = AElf.wallet.createNewWallet();
const wallet = AElf.wallet.getWalletByPrivateKey(defaultPrivateKey);
console.log("wallet.address: " + wallet.address);
// link to local Blockchain, you can learn how to run a local node in https://docs.aelf.io/main/main/setup
// const aelf = new AElf(new AElf.providers.HttpProvider('http://127.0.0.1:1235'));
const aelf = new AElf(new AElf.providers.HttpProvider('http://172.24.74.162:1235'));

if (!aelf.isConnected()) {
  alert('Blockchain Node is not running.');
}

// add event for dom
function initDomEvent(multiTokenContract, CRContract) {
  const CR_Creator = document.getElementById("CR_Creator");
  const CR_Owner = document.getElementById("CR_Owner");
  const CR_Status = document.getElementById("CR_Status");
  const CR_Create = document.getElementById("CR_Create");
  const CR_Login = document.getElementById("CR_Login");
  const CR_Transfer = document.getElementById("CR_Transfer");
  const CR_Register = document.getElementById("CR_Register");
  const target_addr = document.getElementById("target_addr");
  const price = document.getElementById("price");
  const div_create = document.getElementById("create");
  const div_transfer = document.getElementById("transfer");
  const CheckCR = document.getElementById("CheckCR");
  let txId = 0;
  let contractAddr = CRContract.address;
  // Update your card number,Returns the change in the number of your cards
  function getBalance() {
    const payload = {
      symbol: 'ELF',
      owner: wallet.address
    };

    multiTokenContract.GetBalance.call(payload)
      .then(result => {
        console.log('result: ' + result);
      })
      .catch(err => {
        console.log(err);
      });

    return multiTokenContract.GetBalance.call(payload)
      .then(result => {
        console.log(result.balance);
        return result.balance;
      })
      .catch(err => {
        console.log(err);
      });
  }

  // display main UI

  CR_Register.onclick = function() {
    CRContract.CR_Register(wallet.address)
    .then(result => {
      CR_Register.disabled = false;
      console.log(result);
    })
    .catch(err => {
      console.log(err);
    });
  };

  CR_Login.onclick = function() {
    CRContract.CR_Login(wallet.address)
    .then(result => {
      CR_Register.innerText = 'Loading...';
      CR_Register.disabled = true;
      console.log(result);
      
    })
    .then(() => {
      alert('Congratulations on your successful registration！');
      CR_Register.style.display = "none";
      CR_Login.style.display = "none";
      div_create.style.visibility = "visible";
      div_transfer.style.visibility = 'visible';
      console.log(contractAddr);
    })
    .catch(err => {
      console.log(err);
    });
  };
  CheckCR.onclick = function() {
    CRContract.getAllCRTs.call(wallet.address)
    .then(result=>{
      console.log(result);
      CRContract.getAllInfo.call(result.CRT_Set[0])
      .then(ret =>{
        console.log(ret)
      })
    })
    .catch(err => {
      console.log(err);
    })
  }

  CR_Create.onclick = () => {
    CR_Create.innerText = 'Creating...';
    CR_Create.disabled = true;
    multiTokenContract.Approve({
      symbol: 'ELF',
      spender: contractAddr,
      amount: 10000
    })
    .then(
      CRContract.CR_Upload({
        CRT_Creator: CR_Creator.value,
        CRT_Owner: CR_Owner.value,
        CRT_Content: 'Content_test',
        CRT_Status: CR_Status.value
      })
      .then(result => {
        setTimeout(() => {
          CR_Create.innerText = 'CR_Create';
          CR_Create.disabled = false;
          console.log("result: " +result);
          console.log(result.TransactionId);
        }, 1000)
      })
    )
    .catch(err => {
      console.log(err);
    })
  };

  CR_Transfer.onclick = () => {
    CR_Transfer.innerText = 'Transfering...';
    CR_Transfer.disabled = true;
    multiTokenContract.Approve({
      symbol: 'ELF',
      spender: contractAddr,
      amount: 88000000000000000
    })
    .then(
      CRContract.CR_Transfer({
        preID: 'c37a491d67e3fc48058d4a2fd2624e0101fcbdf0aa05dccc765308bfdbb9cf03',
        destAddr: target_addr.value,
        Price: price.value,
        flags: 0
      })
      .then(result => {
        setTimeout(() => {
          CR_Transfer.innerText = 'CR_Transfer';
          CR_Transfer.disabled = false;
          console.log("result: " +result);
          console.log(result.TransactionId);
        }, 1000)
      })
      .catch(err => {
        console.log(err);
      })
    )
  }
}

function init() {
  aelf.chain.getChainStatus()
    // get instance by GenesisContractAddress
    .then(res => aelf.chain.contractAt(res.GenesisContractAddress, wallet))
    // return contract's address which you query by contract's name
    .then(zeroC => zeroC.GetContractAddressByName.call(sha256('AElf.ContractNames.Token')))
    // return contract's instance and you can call the methods on this instance
    .then((tokenAddress) => Promise.all([
      aelf.chain.contractAt(tokenAddress, wallet),
      aelf.chain.contractAt(CRAddress, wallet),
    ]))
    .then(([multiTokenContract, CRContract]) => {
      initDomEvent(multiTokenContract, CRContract);
    })
    .catch(err => {
      console.log(err);
    });
}

// run program
init();