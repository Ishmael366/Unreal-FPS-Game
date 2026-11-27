/* Original browser transport for the Unity studies. Copyright 2026 Daniel Nguyen. MIT. */
(() => {
 let room=null;
 const clamp=(v)=>Math.max(-19,Math.min(19,v));
 const encode=(v)=>btoa(unescape(encodeURIComponent(JSON.stringify(v))));
 const decode=(v)=>JSON.parse(decodeURIComponent(escape(atob(v.trim()))));
 const sendUnity=(r)=>{if(room!==r||!window.unityInstance)return;window.unityInstance.SendMessage(r.receiver,'Receive',JSON.stringify({localId:r.id,status:r.status,invite:r.invite,answer:r.answer,players:Object.values(r.players)}));};
 const ice=(pc)=>new Promise(resolve=>{if(pc.iceGatheringState==='complete')return resolve();const timer=setTimeout(done,8000);function done(){clearTimeout(timer);pc.removeEventListener('icegatheringstatechange',change);resolve();}function change(){if(pc.iceGatheringState==='complete')done();}pc.addEventListener('icegatheringstatechange',change);});
 function setup(receiver,host){
  close();const pc=new RTCPeerConnection({iceServers:[{urls:'stun:stun.l.google.com:19302'}]});
  const r={receiver,host,pc,dc:null,id:host?0:1,players:host?{0:{Id:0,X:0,Z:0,Yaw:0}}:{},input:[0,0],remote:[0,0],seq:0,lastSeq:-1,invite:'',answer:'',status:host?'Creating invitation':'Creating reply'};room=r;
  pc.onconnectionstatechange=()=>{if(room!==r)return;if(pc.connectionState==='failed'||pc.connectionState==='disconnected')r.status='Connection lost — create a new invitation';sendUnity(r);};
  r.timer=setInterval(()=>{
   if(room!==r)return;
   if(r.host){
    const move=(p,input)=>{p.X=clamp(p.X+input[0]*.05*5);p.Z=clamp(p.Z+input[1]*.05*5);if(Math.hypot(...input)>.01)p.Yaw=Math.atan2(input[0],input[1])*180/Math.PI;};
    move(r.players[0],r.input);if(r.players[1])move(r.players[1],r.remote);
    if(r.dc?.readyState==='open')r.dc.send(JSON.stringify({kind:'world',players:Object.values(r.players)}));
   }
   sendUnity(r);
  },50);
  return r;
 }
 function connectChannel(r,dc){
  r.dc=dc;dc.onopen=()=>{if(room!==r)return;r.status='Connected';if(r.host)r.players[1]={Id:1,X:3,Z:0,Yaw:0};sendUnity(r);};
  dc.onclose=()=>{if(room!==r)return;r.status='Guest disconnected';r.remote=[0,0];if(r.host)delete r.players[1];sendUnity(r);};
  dc.onmessage=(event)=>{
   if(room!==r||typeof event.data!=='string'||event.data.length>1024)return;
   try{
    const m=JSON.parse(event.data);
    if(r.host&&m.kind==='input'){
     if(!Number.isSafeInteger(m.seq)||m.seq<=r.lastSeq||!Number.isFinite(m.x)||!Number.isFinite(m.z))return;
     const n=Math.max(1,Math.hypot(m.x,m.z));r.remote=[m.x/n,m.z/n];r.lastSeq=m.seq;
    }else if(!r.host&&m.kind==='world'&&Array.isArray(m.players)&&m.players.length<=2){
     const next={};for(const p of m.players){if(![0,1].includes(p.Id)||![p.X,p.Z,p.Yaw].every(Number.isFinite))return;next[p.Id]=p;}r.players=next;
    }
   }catch{}
  };
 }
 function close(){if(!room)return;const r=room;room=null;clearInterval(r.timer);if(r.dc)r.dc.close();r.pc.close();}
 window.StudyNetwork={
  async host(receiver){let r;try{r=setup(receiver,true);connectChannel(r,r.pc.createDataChannel('study',{ordered:false,maxRetransmits:0}));await r.pc.setLocalDescription(await r.pc.createOffer());await ice(r.pc);if(room!==r)return;r.invite=encode(r.pc.localDescription);r.status='Share the invitation, then paste your guest’s reply';sendUnity(r);}catch(e){if(r){r.status='Unable to create room: '+e.message;sendUnity(r);}}},
  async join(receiver,invite){let r;try{const offer=decode(invite);if(offer.type!=='offer'||typeof offer.sdp!=='string'||offer.sdp.length>40000)throw Error('Invalid invitation');r=setup(receiver,false);r.pc.ondatachannel=e=>connectChannel(r,e.channel);await r.pc.setRemoteDescription(offer);await r.pc.setLocalDescription(await r.pc.createAnswer());await ice(r.pc);if(room!==r)return;r.answer=encode(r.pc.localDescription);r.status='Send this reply to the host';sendUnity(r);}catch(e){if(!r){r=setup(receiver,false);}r.status='Unable to join: '+e.message;sendUnity(r);}},
  async answer(code){const r=room;if(!r||!r.host)return;try{const answer=decode(code);if(answer.type!=='answer'||typeof answer.sdp!=='string'||answer.sdp.length>40000)throw Error('Invalid reply');await r.pc.setRemoteDescription(answer);r.status='Connecting';sendUnity(r);}catch(e){r.status='Unable to connect: '+e.message;sendUnity(r);}},
  input(x,z){const r=room;if(!r||!Number.isFinite(x)||!Number.isFinite(z))return;const n=Math.max(1,Math.hypot(x,z));r.input=[x/n,z/n];if(!r.host&&r.dc?.readyState==='open')r.dc.send(JSON.stringify({kind:'input',seq:++r.seq,x:r.input[0],z:r.input[1]}));},
  close
 };
 addEventListener('beforeunload',close);
})();
