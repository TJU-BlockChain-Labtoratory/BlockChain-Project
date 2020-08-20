import React, {Component} from 'react'
import {View, StyleSheet, Text} from 'react-native'
import RootSiblings from 'react-native-root-siblings'
import pTd from './unit';


let TipView = undefined
/**
 * 信息提示
 */
export default {

    show: (text) => {
        //防止连续两次点击出现提示框不消失
        if (TipView instanceof RootSiblings) {
            return
        }
        TipView = new RootSiblings(
            <View  style={ [StyleSheet.absoluteFill, {justifyContent: 'center', alignItems: 'center',}] } >
                <View style={styles.tipMsgWrap}>
                    <Text style={styles.tipMsg}>{text || " "}</Text>
                </View>
            </View>
        )
    },

    hide: ()=> {
        if (TipView instanceof RootSiblings) {
            TipView.destroy()
            TipView = null
        }
    }

}

const styles = StyleSheet.create({
    tipMsgWrap:{
        backgroundColor:"rgba(0,0,0,0.8)",
        borderRadius:pTd(10),
        justifyContent:"center",
        alignItems:"center",
        padding:pTd(30)
    },
    tipMsg:{
        color:"#fff",
        fontSize:pTd(28)
    }
})
