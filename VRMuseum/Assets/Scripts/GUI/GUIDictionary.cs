using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System;
using System.Globalization;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using Tommy;

namespace vrm {
    public class GUIDictionary : Singleton<GUIDictionary>
    {
        private Dictionary<string, CanvasData> _DictCanvas = new();

        public void Start()
        {
            //Fiat---------------------
            _DictCanvas.Add("Fiat", new CanvasData
            {
                MaxWidth = 2,
                MaxHeight = 3,
                Title = "Fiat Revelli Mod. 1914",
                Paragraphs = new List<ParagraphData> {
                new(cleanStr(@"
                    Il modello Fiat Ravelli 1914 era una mitragliatrice di medie dimensioni utilizzata dagli eserciti nella prima guerra mondiale, è l'arma automatica più usata nella grande guerra in Italia, prodotta a 37.500 esemplari 
 La progettazione dell'arma risale al 1910.
Abiel Revelli modifica la mitragliatrice Perino 1908"".  È stata presentata ad una gara d'appalto indetta dall'esercito reale italiano. Quando viene presentata, non è scelta dalla giuria, ma è Maxim a vincere la gara.
Nel 1913 Abiel Revelli rappresenta la FIAT RAVELLI a Nettuno, e questa volta lo stato maggiore dell'esercito testa nuovamente la mitragliatrice che giudica conforme alle esigenze.
Questa arma è molto robusta, ha una meccanica rustica e una balistica adeguata, ma era molto pesante a causa del suo sistema di raffreddamento ad acqua (17 kg a vuoto e 22 kg riempita d'acqua). Il suo cannone era di 645 mm. Dopo la prima guerra mondiale sarà usata ancora per altre due guerre: le guerre di Spagna e la guerra d'Etiopia.
Nel 1935 sarà sostituita dalla Fiat-Revelli M 1935 che è un'evoluzione della Fiat-Revelli 1914 ma con un nuovo sistema di raffreddamento ad aria, un nuovo calibro portato da 6,5 a 8 mm e infine una nuova alimentazione a nastri metallici al posto del nastro cartuccia.
                "))
                {
                    Audio = "test.wav",
                    Title = "Introduzione"
                },
                new(cleanStr(@"
                    La <b>Fiat-Revelli Mod. 1914<b> e' stata una mitragliatrice media,
                    adottata dal Regio Esercito Italiano nella prima guerra mondiale.
                "))
                {
                    Audio = "test.wav",
                    Title = "Introduzione"
                }
            }
            });
            //Caschetto------------------
            _DictCanvas.Add("Caschetto", new CanvasData
            {
                MaxWidth = 2,
                MaxHeight = 3,
                Title = "Caschetto",
                Paragraphs = new List<ParagraphData> {
                new(cleanStr(@"
                    Questo equipaggiamento comprende un casco d'acciaio di forma ovale munito di una larga piastra frontale di rinforzo e una corazza d'acciaio i suoi artifici di protezione sono fabbricati a Milano dall'ingegnere Farina che dirige parallelamente una piccola fabbrica la fabbrica farina fabbrica un casco simile Elmo da trincea per il resto delle truppe, anche se sembra improbabile che un esercito intero possa essere equipaggiato con un tale Tark. Molto scomodo e di limitata efficacia i caschi Farina non sono apprezzati
Per questo motivo il nostro supremo ordina un importante casco Adrian dalla Francia al fine di utilizzare la situazione, tanto più che l'esplosione di proiettili nelle regioni rocciose costituiscono la maggior parte dei fronti su cui i soldati italiani sono impegnati nell'autunno 1915 in emergenza l'Italia sarà vicina alla Francia per l'importazione di questi caschi per equipaggiare il suo esercito si avanza la cifra di 500000 unità identiche al casco impiegati dai pelosi sul fronte francese colore blu orizzonte con la granata fanteria recante la dicitura RF per Repubblica francese. La fabbricazione potente le nella bomba non è fornita di fessura per l'attaccamento di un attributo su fabbricare appositamente per l'Italia sono in un primo tempo non in grigio blu poi un grigio verde per essere abbinato all'uniforme italiana i caschi modello 15 Adrien degli munides segno frontale metallici sono subito muniti di un distintivo frontale applicato allo stencil per esempio nella fanteria si parte in nero il numero di reggimento sormontato dalla corona reale parallelamente a questo acquisto l'Italia inizia uno studio per la fabbricazione del proprio modello.
                "))
                {
                    Audio = "test.wav",
                    Title = "Introduzione"
                },
            }
            });
            //Pugnale
            _DictCanvas.Add("Pugnale", new CanvasData
            {
                MaxWidth = 2,
                MaxHeight = 3,
                Title = "Pugnale",
                Paragraphs = new List<ParagraphData> {
                new(cleanStr(@"
                    TODO
                "))
                {
                    Audio = "test.wav",
                    Title = "Introduzione"
                }
            }
            });
            //Carcano Mod.91
            _DictCanvas.Add("Carcano", new CanvasData
            {
                MaxWidth = 2,
                MaxHeight = 3,
                Title = "Carcano Mod.91",
                Paragraphs = new List<ParagraphData> {
                new(cleanStr(@"
                    Nel 1914 i bersaglieri avevano una bicicletta pieghevole della marca Bianchi sulla quale trasportava il loro fucile ma poteva trasportare anche una mitragliatrice revelis. Fiat ha aggiunto la staffa per la stabilità durante il tiro di altri cicli erano trasformabili per il servizio sanitario presso lo studio italiano Bianchi con possibilità di mettere borse sotto il telaio. Le biciclette potevano anche essere usate come poltrone operatorie e barelle.
                "))
                {
                    Audio = "test.wav",
                    Title = "Introduzione"
                }
            }
            });
            //Bomba
            _DictCanvas.Add("Bomba", new CanvasData
            {
                MaxWidth = 2,
                MaxHeight = 3,
                Title = "Bomba",
                Paragraphs = new List<ParagraphData> {
                new(cleanStr(@"
                    TODO
                "))
                {
                    Audio = "test.wav",
                    Title = "Introduzione"
                }
            }
            });

        }


        public CanvasData GetCanvasDataByKey(string Key)
        {
            return _DictCanvas[Key];
        }


        private static string cleanStr(string input)
        {
            // Step 1: Remove excess whitespace and literal newlines
            string cleaned = Regex.Replace(input, @"\s+", " ").Trim();
            // Step 2: Insert newlines wherever \n is present
            string result = cleaned.Replace(@"\n", Environment.NewLine);
            return result;
        }
        protected override void OnDestroyCallback()
        {

        }
    }
}
