from pathlib import Path
import struct, shutil, hashlib

p=Path(r'C:\Users\bobya\Documents\ChatGPT\free realms\client-2009-test\UI\ScriptsBase.bin')
backup=p.with_name('ScriptsBase.bin.marketplace-patched')
if not backup.exists(): shutil.copy2(p,backup)
b=bytearray(backup.read_bytes()); pos=12; matches=[]
def take(n):
 global pos
 v=b[pos:pos+n]; pos+=n; return v
def u32(): return struct.unpack('<I',take(4))[0]
def txt():
 n=u32(); return take(n)[:-1].decode('latin1','replace') if n else ''
def proto(parent=''):
 global pos
 src=txt(); take(12); ncode=u32(); code_off=pos; take(ncode*4); const=[]
 for _ in range(u32()):
  t=take(1)[0]
  if t==0: const.append(None)
  elif t==1: const.append(bool(take(1)[0]))
  elif t==3: const.append(struct.unpack('<d',take(8))[0])
  elif t==4: const.append(txt())
  else: raise ValueError((t,pos))
 owner=src or parent
 if owner.lower().endswith('group.lua') and ncode==122 and 'GetActiveProfileId' in const and 'ProfileDsColumns' in const:
  matches.append((code_off,const.index(-1.0),b[code_off:code_off+8].hex()))
 for _ in range(u32()): proto(owner)
 take(u32()*4)
 for _ in range(u32()): txt(); take(8)
 for _ in range(u32()): txt()
proto()
assert len(matches)==1,matches
off,k,old=matches[0]
struct.pack_into('<II',b,off+120*4,1|(14<<6)|(k<<14),30|(14<<6)|(2<<23))
p.write_bytes(b)
print('GetNextJob',hex(off),'original start',old,'exhaustion return',b[off+120*4:off+122*4].hex(),'sha256',hashlib.sha256(b).hexdigest())
