using System.Diagnostics.CodeAnalysis;

namespace SaberLibrary
{
    //Class can't be modified.
	
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class ListNode
    {
        /// <summary>
        /// Ref to the previous node in the list, NULL for head.
        /// </summary>
        public ListNode Previous;

        /// <summary>
        /// Ref to the next node in the list, null for tail.
        /// </summary>
        public ListNode Next;

        /// <summary>
        /// Ref to the random node in the list, could be null.
        /// </summary>
        public ListNode Random;

        /// <summary>
        /// Payload.
        /// </summary>
        public string Data;

        public override string ToString()
        {
            var prevVal = Previous?.Data;
            var nextVal = Next?.Data;
            var randVal = Random?.Data;

            return $"Data: {Data}; Next: {nextVal}; Prev: {prevVal}; Rand: {randVal}";
        }
    }
}
