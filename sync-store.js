/* Browser cache is a recovery copy. Supabase is the account source of truth. */
(function(root){
  'use strict';
  const clone=value=>JSON.parse(JSON.stringify(value));
  class RecallSync {
    constructor({remote,storage,valid,onState,onStatus}) {
      Object.assign(this,{remote,storage,valid,onState,onStatus});
      this.epoch=0;this.context=null;this.inflight=null;this.status='guest';
    }
    key(id){return 'recall-account-v1:'+id;}
    statusChanged(status){this.status=status;this.onStatus(status);}
    cache(c){try{this.storage.setItem(this.key(c.id),JSON.stringify({progress:c.progress,revision:c.revision,dirty:c.dirty}));}catch{if(this.context===c)this.onStatus('cache-error');}}
    canEdit(){return Boolean(this.context?.loaded)&&!['loading','conflict'].includes(this.status);}
    async open(id){
      const epoch=++this.epoch;this.context={id,progress:null,revision:0,dirty:false,generation:0,loaded:false};
      this.statusChanged('loading');let cached=null;
      try{const candidate=JSON.parse(this.storage.getItem(this.key(id))||'null');if(candidate&&this.valid(candidate.progress)&&Number.isInteger(candidate.revision)&&candidate.revision>=0&&typeof candidate.dirty==='boolean')cached=candidate;}catch{}
      try{
        const row=await this.remote.load(id);if(epoch!==this.epoch)return;
        if(row&&(!this.valid(row.progress)||!Number.isInteger(row.revision)||row.revision<1))throw Error('Invalid account progress');
        const revision=row?.revision||0;
        const c=this.context;
        if(cached?.dirty){Object.assign(c,{progress:clone(cached.progress),revision:cached.revision,dirty:true,loaded:true});this.onState(clone(c.progress));if(cached.revision!==revision){this.statusChanged('conflict');return;}}
        else{Object.assign(c,{progress:clone(row?.progress||{version:1,cards:{},log:[]}),revision,loaded:true});this.onState(clone(c.progress));}
        this.cache(c);this.statusChanged(c.dirty?'pending':'saved');if(c.dirty)await this.flush();
      }catch{if(epoch!==this.epoch)return;if(cached){Object.assign(this.context,{progress:clone(cached.progress),revision:cached.revision,dirty:cached.dirty,loaded:true});this.onState(clone(cached.progress));}this.statusChanged('error');}
    }
    close(){++this.epoch;this.context=null;this.statusChanged('guest');}
    change(progress){
      if(!this.canEdit()||!this.valid(progress))throw Error('Account progress is not ready');
      const c=this.context;c.progress=clone(progress);c.dirty=true;c.generation++;this.cache(c);this.statusChanged('pending');
    }
    async flush(){
      const c=this.context;if(!c?.loaded||!c.dirty||this.status==='conflict')return false;
      if(this.inflight?.context===c)return this.inflight.promise;
      const epoch=this.epoch,generation=c.generation,payload=clone(c.progress),revision=c.revision;
      const promise=(async()=>{
        this.statusChanged('saving');
        try{
          const next=await this.remote.save(c.id,payload,revision);
          if(!Number.isInteger(next)||next!==revision+1)throw Error('Unconfirmed save');
          // Keep this account's recovery copy even if sign-out occurred mid-save.
          c.revision=next;c.dirty=c.generation!==generation;this.cache(c);
          if(epoch===this.epoch)this.statusChanged(c.dirty?'pending':'saved');
          return true;
        }catch(e){if(epoch===this.epoch)this.statusChanged(e.conflict?'conflict':'error');return false;}
      })();
      this.inflight={context:c,promise};
      const result=await promise;if(this.inflight?.promise===promise)this.inflight=null;
      if(result&&epoch===this.epoch&&c.dirty)return this.flush();
      return result;
    }
    async retry(){const c=this.context;if(!c)return;if(!c.loaded){await this.open(c.id);return;}await this.flush();if(this.context!==c)return;if(!c.dirty&&this.status==='error')await this.open(c.id);}
    async useCloud(){const c=this.context;if(!c)return;const epoch=this.epoch;this.statusChanged('loading');try{const row=await this.remote.load(c.id);if(epoch!==this.epoch)return;if(row&&(!this.valid(row.progress)||!Number.isInteger(row.revision)||row.revision<1))throw Error('Invalid progress');Object.assign(c,{progress:clone(row?.progress||{version:1,cards:{},log:[]}),revision:row?.revision||0,dirty:false,loaded:true,generation:c.generation+1});this.cache(c);this.onState(clone(c.progress));this.statusChanged('saved');}catch{if(epoch===this.epoch)this.statusChanged('error');}}
  }
  root.RecallSync=RecallSync;
  if(typeof module!=='undefined')module.exports=RecallSync;
})(typeof window!=='undefined'?window:globalThis);

