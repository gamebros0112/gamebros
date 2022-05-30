using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Paroxe.PdfRenderer.WebGL;
using Paroxe.PdfRenderer;

public class PDFLoader : MonoBehaviour
{
    public Renderer targetRenderer;
    public PDFDocument document;
    public int currentPageNum = 0;
    public int totalPageNum = 0;

    // 초기화 
    public void InitPDF(byte[] _pdf)
    {
        StartCoroutine(AttachPDFTexture(_pdf));
    }
    // pdf -> 텍스처로 변환 
    private IEnumerator AttachPDFTexture(byte[] _pdf)
    {
        PDFJS_Promise<PDFDocument> documentPromise = PDFDocument.LoadDocumentFromBytesAsync(_pdf);

        while (!documentPromise.HasFinished)
            yield return null;

        if (!documentPromise.HasSucceeded)
        {
            Debug.Log("Fail: documentPromise");
            yield break;
        }


        document = documentPromise.Result;
        totalPageNum= document.GetPageCount();
        //Debug.Log("document name  : " + gameObject.name + " / totalPageNum : " + totalPageNum);
        PDFJS_Promise<PDFPage> pagePromise = document.GetPageAsync(0);

        while (!pagePromise.HasFinished)
            yield return null;

        if (!pagePromise.HasSucceeded)
        {
            Debug.Log("Fail: pagePromise");

            yield break;
        }


        PDFPage page = pagePromise.Result;

        PDFJS_Promise<Texture2D> renderPromise = PDFRenderer.RenderPageToTextureAsync(page, (int)page.GetPageSize().x, (int)page.GetPageSize().y);

        while (!renderPromise.HasFinished)
            yield return null;

        if (!renderPromise.HasSucceeded)
        {
            Debug.Log("Fail: pagePromise");

            yield break;
        }

        // apply pdf texture
        Texture2D renderedPageTexture = renderPromise.Result;
        targetRenderer.material.SetTexture("_BaseMap", renderedPageTexture);
    }
    // 이전 페이지 
    public void PrevPage()
    {
        if (currentPageNum > 0)
        {
            currentPageNum--;
            StartCoroutine(GotoPDFPage(currentPageNum));
        }

    }
    // 다음 페이지 
    public void NextPage()
    {
        if (currentPageNum < totalPageNum-1)
        {
            currentPageNum++;
            StartCoroutine(GotoPDFPage(currentPageNum));
        }
        
    }
    // 지정한 페이지로 이동 
    IEnumerator GotoPDFPage(int _targetPage)
    {
        PDFJS_Promise<PDFPage> pagePromise = document.GetPageAsync(_targetPage);

        while (!pagePromise.HasFinished)
            yield return null;

        if (!pagePromise.HasSucceeded)
        {
            Debug.Log("Fail: pagePromise");

            yield break;
        }


        PDFPage page = pagePromise.Result;

        PDFJS_Promise<Texture2D> renderPromise = PDFRenderer.RenderPageToTextureAsync(page, (int)page.GetPageSize().x, (int)page.GetPageSize().y);
        while (!renderPromise.HasFinished)
            yield return null;

        if (!renderPromise.HasSucceeded)
        {
            Debug.Log("Fail: pagePromise");

            yield break;
        }
        // apply pdf texture
        Texture2D renderedPageTexture = renderPromise.Result;
        targetRenderer.material.SetTexture("_BaseMap", renderedPageTexture);
    }
}
