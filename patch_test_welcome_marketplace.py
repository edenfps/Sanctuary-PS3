from pathlib import Path
import struct,shutil,hashlib
p=Path(r'C:\Users\bobya\Documents\ChatGPT\free realms\client-2009-test\UI\ScriptsBase.bin')
backup=p.with_name('ScriptsBase.bin.original')
if not backup.exists():shutil.copy2(p,backup)
b=bytearray(p.read_bytes()); pos=12; matches=[]
def take(n):
 global pos
 v=b[pos:pos+n];pos+=n;return v
def u32():return struct.unpack('<I',take(4))[0]
def txt():
 n=u32();return take(n)[:-1].decode('latin1','replace') if n else ''
def proto(parent=''):
 global pos
 src=txt();take(12);ncode=u32();code_off=pos;take(ncode*4);const=[]
 for _ in range(u32()):
  t=take(1)[0]
  if t==0:const.append(None)
  elif t==1:const.append(bool(take(1)[0]))
  elif t==3:const.append(struct.unpack('<d',take(8))[0])
  elif t==4:const.append(txt())
  else:raise ValueError((t,pos))
 owner=src or parent
 if owner.lower().endswith('welcome.lua') and 'InGamePurchaseStoreScreen' in const and 'SelectStoreItemGroup' in const:
  matches.append((code_off,ncode,b[code_off:code_off+4].hex()))
 for _ in range(u32()):proto(owner)
 take(u32()*4)
 for _ in range(u32()):txt();take(8)
 for _ in range(u32()):txt()
proto()
assert len(matches)==1,matches
code_off,ncode,old=matches[0]
print('welcome PopulateMarketplaceItems',hex(code_off),'instructions',ncode,'old',old)
struct.pack_into('<I',b,code_off,0x0080001E)
p.write_bytes(b)
print('patched SHA256',hashlib.sha256(b).hexdigest(),'backup',hashlib.sha256(backup.read_bytes()).hexdigest())