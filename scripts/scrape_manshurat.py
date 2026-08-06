import os
import json
import time
import re
from urllib.parse import urljoin
import requests
from bs4 import BeautifulSoup
from requests.adapters import HTTPAdapter
from urllib3.util.retry import Retry

# Configuration
BASE_URL = "https://manshurat.org"
DOCS_DIR = os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))), "docs", "egyptian law", "manshurat.org")
PROGRESS_FILE = os.path.join(os.path.dirname(os.path.abspath(__file__)), "manshurat_progress.json")
START_NODE = 1
MAX_NODE = 85000

# Setup session with robust retries
session = requests.Session()
retries = Retry(total=5, backoff_factor=1, status_forcelist=[ 500, 502, 503, 504 ])
session.mount('https://', HTTPAdapter(max_retries=retries))
session.mount('http://', HTTPAdapter(max_retries=retries))

def clean_filename(name):
    # Remove invalid characters for Windows paths
    return re.sub(r'[\\/*?:"<>|]', "", name).strip()

def load_progress():
    if os.path.exists(PROGRESS_FILE):
        with open(PROGRESS_FILE, 'r', encoding='utf-8') as f:
            data = json.load(f)
            return data.get("current_node", START_NODE)
    return START_NODE

def save_progress(node_id):
    with open(PROGRESS_FILE, 'w', encoding='utf-8') as f:
        json.dump({"current_node": node_id}, f)

def scrape_node(node_id):
    url = f"{BASE_URL}/node/{node_id}"
    print(f"Fetching Node {node_id}: {url}")
    
    try:
        response = session.get(url, timeout=10)
    except Exception as e:
        print(f"  [ERROR] Connection failed: {e}")
        return False
        
    if response.status_code == 404:
        print(f"  [SKIP] 404 Not Found")
        return True # Successful skip
        
    if response.status_code != 200:
        print(f"  [ERROR] Status code: {response.status_code}")
        return False

    soup = BeautifulSoup(response.content, 'html.parser')
    
    # Extract Title
    title_div = soup.find('div', property='dc:title')
    if not title_div:
        title_tag = soup.find('h2')
        if not title_tag:
            print("  [SKIP] No title found, possibly not a valid document node.")
            return True
        title = title_tag.get_text(strip=True)
    else:
        title = title_div.get_text(strip=True)
        
    if not title:
        title = f"Document_{node_id}"
        
    title = clean_filename(title)
    
    # Extract Category
    category = "General"
    inline_infos = soup.find_all('div', class_='inline-info')
    for info in inline_infos:
        label_div = info.find('div', class_='label-inline')
        if label_div and ("القطاع" in label_div.get_text() or "نوع الوثيقة" in label_div.get_text()):
            lineage_items = info.find_all('span', class_=re.compile(r'lineage-item'))
            if lineage_items:
                # Take the most specific category
                category = clean_filename(lineage_items[-1].get_text(strip=True))
                break
                
    # Prepare directory
    cat_dir = os.path.join(DOCS_DIR, category)
    os.makedirs(cat_dir, exist_ok=True)
    
    saved_something = False

    # Extract HTML Text
    content_div = soup.find('div', property='content:encoded')
    if content_div:
        text_content = content_div.get_text(separator='\n', strip=True)
        if text_content:
            txt_path = os.path.join(cat_dir, f"{title}.txt")
            with open(txt_path, 'w', encoding='utf-8') as f:
                f.write(text_content)
            print(f"  [SAVED TXT] {txt_path}")
            saved_something = True

    # Extract PDF if available
    pdf_link = soup.find('a', attrs={'type': 'application/pdf'})
    if not pdf_link:
        # Fallback check
        pdf_link = soup.find('a', href=re.compile(r'/file/\d+/download'))
        
    if pdf_link and pdf_link.has_attr('href'):
        pdf_url = urljoin(BASE_URL, pdf_link['href'])
        pdf_path = os.path.join(cat_dir, f"{title}.pdf")
        
        if not os.path.exists(pdf_path):
            try:
                pdf_res = session.get(pdf_url, stream=True, timeout=15)
                if pdf_res.status_code == 200:
                    with open(pdf_path, 'wb') as f:
                        for chunk in pdf_res.iter_content(chunk_size=8192):
                            f.write(chunk)
                    print(f"  [SAVED PDF] {pdf_path}")
                    saved_something = True
                else:
                    print(f"  [ERROR] PDF download returned {pdf_res.status_code}")
            except Exception as e:
                print(f"  [ERROR] Failed to download PDF: {e}")
        else:
            print(f"  [SKIP PDF] Already exists: {pdf_path}")
            saved_something = True
            
    if not saved_something:
        print("  [SKIP] No text or PDF found on this page.")
        
    return True

def main():
    os.makedirs(DOCS_DIR, exist_ok=True)
    start = load_progress()
    
    print(f"Starting scrape from node {start}...")
    
    for node_id in range(start, MAX_NODE + 1):
        success = scrape_node(node_id)
        if success:
            save_progress(node_id + 1)
        
        # Sleep to avoid overwhelming the server
        time.sleep(0.5)

if __name__ == "__main__":
    main()
