/**
 * @file index.js
 * @author zmh3788
 * @description none
*/

import AElf from 'aelf-sdk';

const { sha256 } = AElf.utils;

const defaultPrivateKey = '845dadc4609852818f3f7466b63adad0504ee77798b91853fdab6af80d3a4eba';
// const wallet = AElf.wallet.createNewWallet();
const wallet = AElf.wallet.getWalletByPrivateKey(defaultPrivateKey);
console.log("wallet.address: " + wallet.address);
// link to local Blockchain, you can learn how to run a local node in https://docs.aelf.io/main/main/setup
// const aelf = new AElf(new AElf.providers.HttpProvider('http://127.0.0.1:1235'));
const aelf = new AElf(new AElf.providers.HttpProvider('http://127.0.0.1:1235'));

if (!aelf.isConnected()) {
  alert('Blockchain Node is not running.');
}

// add event for dom
function initDomEvent(multiTokenContract, CopyRightContract) {
  const CRT_Creator = document.getElementById("CRT_Creator");
  const CRT_Owner = document.getElementById("CRT_Owner");
  const CRT_Status = document.getElementById("CRT_Status");
  const CRT_Create = document.getElementById("CRT_Create");
  const CRT_Transfer = document.getElementById("CRT_Transfer");
  const CRT_Register = document.getElementById("CRT_Register");
  const target_addr = document.getElementById("target_addr");
  const price = document.getElementById("price");
  const div_create = document.getElementById("create");
  const div_transfer = document.getElementById("transfer");

  let txId = 0;
  let contractAddr = CopyRightContract.address;
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
  let loading = false;
  let value = 88000000000000000;
  CRT_Register.onclick = function() {
    CopyRightContract.Register()
      .then(() => {
        CRT_Register.innerText = 'Loading...';
        CRT_Register.disabled = true;
        return new Promise(resolve => {
          setTimeout(() => {
            getBalance();
            loading = false;
            resolve()
          }, 1500);
        });
      })
      .then(() => {
        alert('Congratulations on your successful registration！');
        CRT_Register.style.display = "none";
        div_create.style.visibility = "visible";
        div_transfer.style.visibility = 'visible';
        console.log(contractAddr);
      })
      .catch(err => {
        console.log(err);
      });
  };

  CRT_Create.onclick = () => {
    CRT_Create.innerText = 'Creating...';
    CRT_Create.disabled = true;
    multiTokenContract.Approve({
      symbol: 'ELF',
      spender: contractAddr,
      amount: 88000000000000000
    })
    .then(
      CopyRightContract.CR_Upload({
        Address: wallet.address,
        Creater: {name: CRT_Owner.value},
        ContentHash: 'Content_test',
        flags: 0
      })
      .then(result => {
        setTimeout(() => {
          CRT_Create.innerText = 'CRT_Create';
          CRT_Create.disabled = false;
          console.log("result: " +result);
          console.log(result.TransactionId);
        }, 1000)
      })
      .catch(err => {
        console.log(err);
      })
    );
  };

  CRT_Transfer.onclick = () => {
    CRT_Transfer.innerText = 'Transfering...';
    CRT_Transfer.disabled = true;
    multiTokenContract.Approve({
      symbol: 'ELF',
      spender: contractAddr,
      amount: 88000000000000000
    })
    .then(
      CopyRightContract.CR_Transfer({
        preID: 'c37a491d67e3fc48058d4a2fd2624e0101fcbdf0aa05dccc765308bfdbb9cf03',
        destAddr: target_addr.value,
        Price: price.value,
        flags: 0
      })
      .then(result => {
        setTimeout(() => {
          CRT_Transfer.innerText = 'CRT_Transfer';
          CRT_Transfer.disabled = false;
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
    .then(zeroC => Promise.all([
      zeroC.GetContractAddressByName.call(sha256('AElf.ContractNames.Token')),
      zeroC.GetContractAddressByName.call(sha256('AElf.ContractNames.CopyRightContract')),
     // console.log(zeroC)
    ]))
    // return contract's instance and you can call the methods on this instance
    .then(([tokenAddress, CopyRightAddress]) => Promise.all([
      aelf.chain.contractAt(tokenAddress, wallet),
      aelf.chain.contractAt(CopyRightAddress, wallet)
    ]))
    .then(([multiTokenContract , CopyRightContract]) => {
      window.CopyRightContract = CopyRightContract;
      initDomEvent(multiTokenContract, CopyRightContract);
    })
    .catch(err => {
      console.log(err);
    });
}

// run program
init();